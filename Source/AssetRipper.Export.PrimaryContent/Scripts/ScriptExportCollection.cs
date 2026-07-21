using AsmResolver.DotNet;
using AssetRipper.Assets;
using AssetRipper.Export.Scripts;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;

namespace AssetRipper.Export.PrimaryContent.Scripts;

public sealed class ScriptExportCollection : ExportCollectionBase
{
	public ScriptExportCollection(ScriptContentExtractor contentExtractor, LanguageVersion languageVersion = LanguageVersion.Latest)
	{
		ContentExtractor = contentExtractor;
		LanguageVersion = languageVersion;
	}

	public override ScriptContentExtractor ContentExtractor { get; }

	public LanguageVersion LanguageVersion { get; }

	public override IEnumerable<IUnityObjectBase> Assets => [];

	public override string Name => nameof(ScriptExportCollection);

	public override bool Contains(IUnityObjectBase asset) => asset is IMonoScript;

	public override bool Export(string projectDirectory, FileSystem fileSystem)
	{
		IAssemblyManager assemblyManager = ContentExtractor.AssemblyManager;
		ILSpyAssemblyResolver assemblyResolver = new(assemblyManager);

		string assemblyDirectory = fileSystem.Path.Join(projectDirectory, "Assemblies");
		fileSystem.Directory.Create(assemblyDirectory);

		//Export assemblies
		foreach (AssemblyDefinition assembly in assemblyManager.GetAssemblies())
		{
			Stream stream = assemblyManager.GetStreamForAssembly(assembly);
			stream.Position = 0;

			//Write assembly
			{
				string assemblyPath = fileSystem.Path.Join(assemblyDirectory, assembly.Name + ".dll");
				using Stream fileStream = fileSystem.File.Create(assemblyPath);
				stream.CopyTo(fileStream);
				stream.Position = 0;
			}
		}

		//Decompile scripts
		string scriptDirectory = fileSystem.Path.Join(projectDirectory, "Scripts");
		foreach (AssemblyDefinition assembly in assemblyManager.GetAssemblies())
		{
			string assemblyName = assembly.Name ?? throw new InvalidOperationException("Assembly name is null");
			string outputDirectory = fileSystem.Path.Join(scriptDirectory, assemblyName);
			fileSystem.Directory.Create(outputDirectory);

			DecompilerSettings settings = new();

			settings.SetLanguageVersion(LanguageVersion);

			settings.AlwaysShowEnumMemberValues = true;
			settings.ShowXmlDocumentation = true;

			settings.UseNestedDirectoriesForNamespaces = true;

			WholeProjectDecompiler decompiler = new(settings, assemblyResolver, null, null, null);
			decompiler.DecompileProject(assemblyResolver.Resolve(assembly), outputDirectory);
		}

		return true;
	}
}
