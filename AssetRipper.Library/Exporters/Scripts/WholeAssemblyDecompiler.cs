using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.Util;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using static ICSharpCode.Decompiler.Metadata.MetadataExtensions;

namespace AssetRipper.Library.Exporters.Scripts
{
	/// <summary>
	/// Decompiles an assembly into a visual studio project file.
	/// </summary>
	public sealed class WholeAssemblyDecompiler
	{
		/// <summary>
		/// Gets the setting this instance uses for decompiling.
		/// </summary>
		public DecompilerSettings Settings { get; } = new();

		public IAssemblyResolver AssemblyResolver { get; }

		public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;

		public IProgress<DecompilationProgress>? ProgressIndicator { get; set; }

		public List<IAstTransform> CustomTransforms { get; } = new();

		// per-run members
		private readonly HashSet<string> directories = new HashSet<string>(Platform.FileNameComparer);

		public WholeAssemblyDecompiler(IAssemblyResolver assemblyResolver)
		{
			AssemblyResolver = assemblyResolver ?? throw new ArgumentNullException(nameof(assemblyResolver));
		}

		public void DecompileProject(PEFile moduleDefinition, string targetDirectory, CancellationToken cancellationToken = default)
		{
			directories.Clear();
			WriteCodeFilesInProject(moduleDefinition, targetDirectory, cancellationToken);
		}

		private bool IncludeTypeWhenDecompilingProject(PEFile module, TypeDefinitionHandle type)
		{
			MetadataReader metadata = module.Metadata;
			TypeDefinition typeDef = metadata.GetTypeDefinition(type);
			if (metadata.GetString(typeDef.Name) == "<Module>" || CSharpDecompiler.MemberIsHidden(module, type, Settings))
			{
				return false;
			}

			if (metadata.GetString(typeDef.Namespace) == "XamlGeneratedNamespace" && metadata.GetString(typeDef.Name) == "GeneratedInternalTypeHelper")
			{
				return false;
			}

			return true;
		}

		private void WriteCodeFilesInProject(PEFile module, string targetDirectory, CancellationToken cancellationToken)
		{
			List<IGrouping<string, TypeDefinitionHandle>> files = GetFiles(module, targetDirectory);
			DecompilerTypeSystem ts = new DecompilerTypeSystem(module, AssemblyResolver, Settings);

			if (CustomTransforms.Count > 0) //Transforms are not thread-safe
			{
				DecompileInSerial(files, module, ts, targetDirectory, ProgressIndicator, cancellationToken);
			}
			else
			{
				DecompileInParallel(files, module, ts, targetDirectory, ProgressIndicator, cancellationToken);
			}
		}

		private List<IGrouping<string, TypeDefinitionHandle>> GetFiles(PEFile module, string targetDirectory)
		{
			MetadataReader metadata = module.Metadata;
			return metadata
				.GetTopLevelTypeDefinitions()
				.Where(td => IncludeTypeWhenDecompilingProject(module, td))
				.GroupBy(
					(TypeDefinitionHandle handle) => GetOutputPath(handle, metadata, targetDirectory),
					StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		private string GetOutputPath(TypeDefinitionHandle handle, MetadataReader metadata, string targetDirectory)
		{
			TypeDefinition type = metadata.GetTypeDefinition(handle);
			string file = FilePathCleanup.CleanUpFileName(metadata.GetString(type.Name)) + ".cs";
			string ns = metadata.GetString(type.Namespace);
			if (string.IsNullOrEmpty(ns))
			{
				return file;
			}
			else
			{
				string dir = Settings.UseNestedDirectoriesForNamespaces ? FilePathCleanup.CleanUpPath(ns) : FilePathCleanup.CleanUpDirectoryName(ns);
				if (directories.Add(dir))
				{
					Directory.CreateDirectory(Path.Combine(targetDirectory, dir));
				}

				return Path.Combine(dir, file);
			}
		}

		private void DecompileInParallel(
			List<IGrouping<string, TypeDefinitionHandle>> files,
			PEFile module,
			DecompilerTypeSystem ts,
			string targetDirectory,
			IProgress<DecompilationProgress>? progress,
			CancellationToken cancellationToken)
		{
			int total = files.Count;
			Parallel.ForEach(
				Partitioner.Create(files, loadBalance: true),
				new ParallelOptions
				{
					MaxDegreeOfParallelism = MaxDegreeOfParallelism,
					CancellationToken = cancellationToken
				},
				(IGrouping<string, TypeDefinitionHandle> file) => DecompileSingleFile(file, module, ts, targetDirectory, total, progress, cancellationToken));
		}

		private void DecompileInSerial(
			List<IGrouping<string, TypeDefinitionHandle>> files,
			PEFile module,
			DecompilerTypeSystem ts,
			string targetDirectory,
			IProgress<DecompilationProgress>? progress,
			CancellationToken cancellationToken)
		{
			int total = files.Count;
			for (int i = 0; i < total; i++)
			{
				IGrouping<string, TypeDefinitionHandle> file = files[i];
				DecompileSingleFile(file, module, ts, targetDirectory, total, progress, cancellationToken);
			}
		}

		private void DecompileSingleFile(
			IGrouping<string, TypeDefinitionHandle> file,
			PEFile module,
			DecompilerTypeSystem ts,
			string targetDirectory,
			int total,
			IProgress<DecompilationProgress>? progress,
			CancellationToken cancellationToken)
		{
			using StreamWriter w = new StreamWriter(Path.Combine(targetDirectory, file.Key));
			try
			{
				CSharpDecompiler decompiler = CreateDecompiler(ts);
				decompiler.CancellationToken = cancellationToken;
				SyntaxTree syntaxTree = decompiler.DecompileTypes(file.ToArray());
				syntaxTree.AcceptVisitor(new CSharpOutputVisitor(w, Settings.CSharpFormattingOptions));
			}
			catch (Exception innerException) when (innerException is not OperationCanceledException && innerException is not DecompilerException)
			{
				throw new DecompilerException(module, $"Error decompiling for '{file.Key}'", innerException);
			}
			progress?.Report(new DecompilationProgress(total, file.Key));
		}

		private CSharpDecompiler CreateDecompiler(DecompilerTypeSystem ts)
		{
			CSharpDecompiler decompiler = new CSharpDecompiler(ts, Settings);
			decompiler.AstTransforms.Add(new EscapeInvalidIdentifiers());
			decompiler.AstTransforms.Add(new RemoveCLSCompliantAttribute());
			for (int i = 0; i < CustomTransforms.Count; i++)
			{
				decompiler.AstTransforms.Add(CustomTransforms[i]);
			}
			return decompiler;
		}
	}
}
