using AsmResolver.DotNet;
using AssetRipper.Assets;
using AssetRipper.Export.Scripts;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using System.Xml;

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
		List<string> assemblyNames = [];
		foreach (AssemblyDefinition assembly in assemblyManager.GetAssemblies())
		{
			string assemblyName = assembly.Name ?? throw new InvalidOperationException("Assembly name is null");
			Logger.Info(LogCategory.Export, $"Decompiling assembly {assemblyName}...");
			string outputDirectory = fileSystem.Path.Join(scriptDirectory, assemblyName);
			fileSystem.Directory.Create(outputDirectory);

			DecompilerSettings settings = new();

			settings.SetLanguageVersion(LanguageVersion);

			settings.AlwaysShowEnumMemberValues = true;
			settings.ShowXmlDocumentation = true;

			settings.UseNestedDirectoriesForNamespaces = true;

			if (assemblyName is "mscorlib")
			{
				// Disable tuple types (the "(int, string)" syntax) for mscorlib to avoid compilation issues.
				// System.Private.CoreLib doesn't seem to use tuple types, and trying to use them in mscorlib causes errors.
				settings.TupleTypes = false;
			}

			try
			{
				ILSpyWholeProjectDecompiler decompiler = new(settings, assemblyResolver, ProjectFileWriter.Instance, fileSystem);
				decompiler.DecompileProject(assemblyResolver.Resolve(assembly), outputDirectory);
			}
			catch (Exception exception)
			{
				Logger.Error(exception);
			}

			assemblyNames.Add(assemblyName);
		}

		// Write solution file
		if (assemblyNames.Count > 0)
		{
			assemblyNames.Sort(StringComparer.Ordinal);

			string solutionPath = fileSystem.Path.Join(scriptDirectory, "Scripts.slnx");
			using Stream stream = fileSystem.File.Create(solutionPath);
			using StreamWriter streamWriter = new(stream)
			{
				NewLine = "\n",
				AutoFlush = true
			};
			using XmlTextWriter xmlWriter = new(streamWriter)
			{
				Formatting = Formatting.Indented
			};

			xmlWriter.WriteStartElement("Solution");

			foreach (string assemblyName in assemblyNames)
			{
				xmlWriter.WriteStartElement("Project");
				xmlWriter.WriteAttributeString("Path", fileSystem.Path.Join(assemblyName, assemblyName + ".csproj"));
				xmlWriter.WriteEndElement();
			}

			xmlWriter.WriteEndElement();
		}

		return true;
	}
}
