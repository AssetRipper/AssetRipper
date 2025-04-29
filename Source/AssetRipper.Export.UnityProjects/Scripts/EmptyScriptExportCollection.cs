using AssetRipper.Export.UnityProjects.Scripts.AssemblyDefinitions;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using System.Diagnostics;

namespace AssetRipper.Export.UnityProjects.Scripts;

public sealed class EmptyScriptExportCollection : ScriptExportCollectionBase
{
	public EmptyScriptExportCollection(ScriptExporter assetExporter, IMonoScript firstScript) : base(assetExporter, firstScript)
	{
		Debug.Assert(!assetExporter.AssemblyManager.IsSet);

		// Find all scripts in the project
		foreach (IMonoScript assetScript in firstScript.Collection.Bundle.FetchAssetsInHierarchy().OfType<IMonoScript>())
		{
			UniqueScripts.TryAdd(MonoScriptInfo.From(assetScript), assetScript);
		}
	}

	private Dictionary<MonoScriptInfo, IMonoScript> UniqueScripts { get; } = new();

	public override string Name => nameof(EmptyScriptExportCollection);

	public override bool Export(IExportContainer container, string projectDirectory, FileSystem fileSystem)
	{
		Logger.Info(LogCategory.Export, "Exporting scripts...");

		string assetsDirectoryPath = fileSystem.Path.Join(projectDirectory, AssetsKeyword);

		Dictionary<string, AssemblyDefinitionDetails> assemblyDefinitionDetailsDictionary = new();

		foreach ((MonoScriptInfo info, IMonoScript script) in UniqueScripts)
		{
			if (info.IsInjected())
			{
				continue;
			}

			GetExportSubPath(info, out string subFolderPath, out string fileName);
			string folderPath = fileSystem.Path.Join(assetsDirectoryPath, subFolderPath);
			string filePath = fileSystem.Path.Join(folderPath, fileName);
			fileSystem.Directory.Create(folderPath);
			fileSystem.File.WriteAllText(filePath, EmptyScript.GetContent(info));
			string assemblyName = info.Assembly;
			if (!assemblyDefinitionDetailsDictionary.ContainsKey(assemblyName))
			{
				string assemblyDirectoryPath = fileSystem.Path.Join(assetsDirectoryPath, GetScriptsFolderName(assemblyName), assemblyName);
				AssemblyDefinitionDetails details = new AssemblyDefinitionDetails(assemblyName, assemblyDirectoryPath);
				assemblyDefinitionDetailsDictionary.Add(assemblyName, details);
			}

			OnScriptExported(container, script, filePath, fileSystem);
		}

		// assembly definitions were added in 2017.3
		//     see: https://blog.unity.com/technology/unity-2017-3b-feature-preview-assembly-definition-files-and-transform-tool
		if (assemblyDefinitionDetailsDictionary.Count > 0 && container.ExportVersion.GreaterThanOrEquals(2017, 3))
		{
			foreach (AssemblyDefinitionDetails details in assemblyDefinitionDetailsDictionary.Values)
			{
				// exclude predefined assemblies like Assembly-CSharp.dll
				//    see: https://docs.unity3d.com/2017.3/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html
				if (!ReferenceAssemblies.IsPredefinedAssembly(details.AssemblyName))
				{
					AssemblyDefinitionExporter.Export(details, fileSystem);
				}
			}
		}

		return true;
	}
}
