using AssetRipper.Assets;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.Metadata;
using System.Runtime.InteropServices;
using AssemblyDefinition = AsmResolver.DotNet.AssemblyDefinition;

namespace AssetRipper.Export.PrimaryContent.Scripts;

public sealed class ScriptExportCollection : ExportCollectionBase
{
	public ScriptExportCollection(ScriptContentExtractor contentExtractor, LanguageVersion languageVersion = LanguageVersion.Latest)
	{
		ContentExtractor = contentExtractor;
		LanguageVersion = languageVersion;
	}

	public override IContentExtractor ContentExtractor { get; }

	public LanguageVersion LanguageVersion { get; }

	public override IEnumerable<IUnityObjectBase> Assets => [];

	public override string Name => nameof(ScriptExportCollection);

	public override bool Contains(IUnityObjectBase asset) => asset is IMonoScript;

	public override bool Export(string projectDirectory, FileSystem fileSystem)
	{
		IAssemblyManager assemblyManager = ((ScriptContentExtractor)ContentExtractor).AssemblyManager;

		string assemblyDirectory = fileSystem.Path.Join(projectDirectory, "Assemblies");
		fileSystem.Directory.Create(assemblyDirectory);

		//Export assemblies
		List<string> assemblyPaths = new();
		foreach (AssemblyDefinition assembly in assemblyManager.GetAssemblies())
		{
			Stream stream = assemblyManager.GetStreamForAssembly(assembly);
			stream.Position = 0;

			//Write assembly
			{
				string assemblyPath = fileSystem.Path.Join(assemblyDirectory, assembly.Name + ".dll");
				assemblyPaths.Add(assemblyPath);
				using Stream fileStream = fileSystem.File.Create(assemblyPath);
				stream.CopyTo(fileStream);
				stream.Position = 0;
			}
		}

		//Decompile scripts
		string scriptDirectory = fileSystem.Path.Join(projectDirectory, "Scripts");
		foreach (string assemblyPath in assemblyPaths)
		{
			string assemblyName = fileSystem.Path.GetFileNameWithoutExtension(assemblyPath);
			string outputDirectory = fileSystem.Path.Join(scriptDirectory, assemblyName);
			fileSystem.Directory.Create(outputDirectory);

			DecompilerSettings settings = new();

			settings.SetLanguageVersion(LanguageVersion);

			settings.AlwaysShowEnumMemberValues = true;
			settings.ShowXmlDocumentation = true;

			settings.UseSdkStyleProjectFormat = false;//sdk style can throw
			settings.UseNestedDirectoriesForNamespaces = true;

			DeterministicWholeProjectDecompiler decompiler = new(settings, new UniversalAssemblyResolver(assemblyPath, false, null));
			PEFile file = new(assemblyPath);
			decompiler.DecompileProject(file, outputDirectory);
		}

		return true;
	}

	private sealed class DeterministicWholeProjectDecompiler(DecompilerSettings settings, IAssemblyResolver assemblyResolver)
		: WholeProjectDecompiler(settings, NextRandomGuid(), assemblyResolver, null, null, null)
	{
		/// <remarks>
		/// If the user has set <c>SOURCE_DATE_EPOCH</c>, they must want deterministic output,
		/// which we can provide by using their timestamp (base-10 s64 string) as the PRNG seed.
		/// <br/>For more details on how tools can use <c>SOURCE_DATE_EPOCH</c> to improve build reproducibility,
		/// see <see href="https://reproducible-builds.org/docs/source-date-epoch/#setting-the-variable">reproducible-builds.org</see>.
		/// </remarks>
		private static readonly Random rng = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH"))
			&& long.TryParse(Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH"), out long l)
				? new(Seed: unchecked((int)l))
				: Random.Shared;

		public static Guid NextRandomGuid()
		{
			Span<byte> buf = stackalloc byte[/*sizeof(Guid)*/16];
			rng.NextBytes(buf);
			return MemoryMarshal.Read<Guid>(buf);
		}
	}
}
