/*using AsmResolver.DotNet;
using AsmResolver.DotNet.Cloning;
using AsmResolver.DotNet.Signatures;
using AssetRipper.CIL;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Text;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// Generates polyfills for missing attributes using the Roslyn compiler.
/// During compilation, these attributes can be stripped because they're not "custom" attributes.
/// They're special and get encoded in the metadata directly.
/// </summary>
/// <remarks>
/// <see href="https://github.com/icsharpcode/ILSpy/blob/master/ICSharpCode.Decompiler/TypeSystem/Implementation/KnownAttributes.cs"/>
/// </remarks>
public sealed class AttributePolyfillGenerator : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);
	private static void Process(IAssemblyManager manager)
	{
		ModuleDefinition? mscorlib = manager.GetAssemblies().FirstOrDefault(a => a.Name == "mscorlib")?.ManifestModule;
		if (mscorlib is null)
		{
			return;
		}

		List<SyntaxTree> syntaxTrees = [];

		foreach (AttributePolyfill polyfill in AttributePolyfill.GetPolyfills())
		{
			if (!mscorlib.HasTopLevelType(polyfill.Namespace, polyfill.Name))
			{
				AddCode(syntaxTrees, polyfill.Code);
			}
		}

		if (syntaxTrees.Count == 0)
		{
			return;
		}

		MetadataReference mscorlibMetadataReference = CreateMetadataReference(manager, mscorlib.Assembly!);

		using MemoryStream polyfillOutputStream = new();

		// Emit compiled assembly into MemoryStream
		CSharpCompilation compilation = CreateCompilation(syntaxTrees, [mscorlibMetadataReference]);
		EmitResult result = compilation.Emit(polyfillOutputStream);

		if (!result.Success)
		{
			Logger.Error(LogCategory.Processing, "Polyfill compilation failed!");
		}
		else
		{
			ModuleDefinition polyfillModule = ModuleDefinition.FromBytes(polyfillOutputStream.ToArray());

			MemberCloner cloner = new(mscorlib, (context) => new CustomCloneReferenceImporter(context));
			cloner.Include(polyfillModule.GetAllTypes().Where(t => t.Namespace is not null));

			MemberCloneResult cloneResult = cloner.Clone();
			foreach (TypeDefinition type in cloneResult.ClonedTopLevelTypes)
			{
				mscorlib.TopLevelTypes.Add(type);
			}

			manager.ClearStreamCache();
		}
	}

	private sealed class CustomCloneReferenceImporter : CloneContextAwareReferenceImporter
	{
		public CustomCloneReferenceImporter(MemberCloneContext context) : base(context)
		{
		}

		protected override ITypeDefOrRef ImportType(TypeDefinition type)
		{
			if (type.DeclaringType is null && TargetModule.TryGetTopLevelType(type.Namespace, type.Name, out TypeDefinition? typeDefinition))
			{
				// This shouldn't happen. My original intention was in regards to Microsoft.CodeAnalysis.EmbeddedAttribute, but it doesn't seem to be present.
				Logger.Warning(LogCategory.Processing, $"Type {type.FullName} already exists in the target module");
				return typeDefinition;
			}
			return base.ImportType(type);
		}

		protected override ITypeDefOrRef ImportType(TypeReference type)
		{
			if (SignatureComparer.Default.Equals(type.Scope, TargetModule) &&
				type.DeclaringType is null &&
				TargetModule.TryGetTopLevelType(type.Namespace, type.Name, out TypeDefinition? typeDefinition))
			{
				return typeDefinition;
			}
			return base.ImportType(type);
		}
	}

	private static void AddCode(List<SyntaxTree> syntaxTrees, string code)
	{
		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code, encoding: Encoding.UTF8);
		syntaxTrees.Add(syntaxTree);
	}

	private static CSharpCompilation CreateCompilation(IEnumerable<SyntaxTree> syntaxTrees, IEnumerable<MetadataReference> references)
	{
		// Define compilation options
		CSharpCompilationOptions compilationOptions = new(OutputKind.DynamicallyLinkedLibrary);

		// Create the compilation
		CSharpCompilation compilation = CSharpCompilation.Create(
			"System.Polyfill",
			syntaxTrees,
			references,
			compilationOptions
		);
		return compilation;
	}

	private static MetadataReference CreateMetadataReference(IAssemblyManager manager, AssemblyDefinition assembly)
	{
		Stream stream = manager.GetStreamForAssembly(assembly);
		stream.Position = 0;

		// We need to copy the stream to a memory stream to prevent it from being disposed
		MemoryStream memoryStream = new();
		stream.CopyTo(memoryStream);
		memoryStream.Position = 0;

		return MetadataReference.CreateFromStream(memoryStream, filePath: $"{assembly.Name}.dll");
	}
}*/
