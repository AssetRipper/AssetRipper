using AsmResolver.DotNet;
using AssetRipper.Assets;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.Metadata;

namespace AssetRipper.Export.PrimaryContent.Scripts;

public sealed class ScriptExportCollection : ExportCollectionBase
{
	public ScriptExportCollection(ScriptContentExtractor contentExtractor)
	{
		ContentExtractor = contentExtractor;
	}

	public override IContentExtractor ContentExtractor { get; }

	public override IEnumerable<IUnityObjectBase> Assets => [];

	public override string Name => nameof(ScriptExportCollection);

	public override bool Contains(IUnityObjectBase asset) => asset is IMonoScript;

	public override bool Export(string projectDirectory)
	{
		IAssemblyManager assemblyManager = ((ScriptContentExtractor)ContentExtractor).AssemblyManager;

		string assemblyDirectory = Path.Combine(projectDirectory, "Assemblies");
		Directory.CreateDirectory(assemblyDirectory);

		//Export assemblies
		List<string> assemblyPaths = new();
		foreach (AssemblyDefinition assembly in assemblyManager.GetAssemblies())
		{
			Stream stream = assemblyManager.GetStreamForAssembly(assembly);
			stream.Position = 0;

			//Write assembly
			{
				string assemblyPath = Path.Combine(assemblyDirectory, assembly.Name + ".dll");
				assemblyPaths.Add(assemblyPath);
				using FileStream fileStream = File.Create(assemblyPath);
				stream.CopyTo(fileStream);
				stream.Position = 0;
			}
		}

		//Decompile scripts
		string scriptDirectory = Path.Combine(projectDirectory, "Scripts");
		foreach (string assemblyPath in assemblyPaths)
		{
			string assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
			string outputDirectory = Path.Combine(scriptDirectory, assemblyName);
			Directory.CreateDirectory(outputDirectory);
			WholeProjectDecompiler decompiler = new(new UniversalAssemblyResolver(assemblyPath, false, null));
			PEFile file = new(assemblyPath);
			decompiler.DecompileProject(file, outputDirectory);
		}

		return true;
	}
}
