using AsmResolver.DotNet;
using AssetRipper.Assets;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using ICSharpCode.Decompiler;
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

			settings.AlwaysShowEnumMemberValues = true;
			settings.ShowXmlDocumentation = true;

			settings.UseSdkStyleProjectFormat = false;//sdk style can throw
			settings.UseNestedDirectoriesForNamespaces = true;

			WholeProjectDecompiler decompiler = new(settings, new UniversalAssemblyResolver(assemblyPath, false, null), null, null, null);
			PEFile file = new(assemblyPath);
			decompiler.DecompileProject(file, outputDirectory);
		}

		return true;
	}
}
