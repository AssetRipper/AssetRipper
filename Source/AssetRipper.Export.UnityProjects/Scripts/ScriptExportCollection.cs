using AsmResolver.DotNet;
using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Export.UnityProjects.Scripts.AssemblyDefinitions;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files.Utils;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Classes.ClassID_1035;
using AssetRipper.SourceGenerated.Classes.ClassID_1050;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using System.Diagnostics;

namespace AssetRipper.Export.UnityProjects.Scripts
{
	public partial class ScriptExportCollection : ExportCollection
	{
		public ScriptExportCollection(ScriptExporter assetExporter, Bundle bundle)
		{
			AssetExporter = assetExporter;

			// find copies in whole project and skip them
			Dictionary<MonoScriptInfo, IMonoScript> uniqueDictionary = new();
			foreach (IMonoScript assetScript in bundle.FetchAssetsInHierarchy().OfType<IMonoScript>())
			{
				MonoScriptInfo info = MonoScriptInfo.From(assetScript);
				if (uniqueDictionary.TryGetValue(info, out IMonoScript? uniqueScript))
				{
					m_scripts.Add(assetScript, uniqueScript);
				}
				else
				{
					m_scripts.Add(assetScript, assetScript);
					uniqueDictionary.Add(info, assetScript);
					if (ShouldExport(assetScript))
					{
						m_export.Add(assetScript);
					}
				}
			}
		}

		private bool ShouldExport(IMonoScript script)
		{
			if (AssetExporter.GetExportType(script.GetAssemblyNameFixed()) is AssemblyExportType.Decompile)
			{
				return !AssetExporter.AssemblyManager.IsSet || script.IsScriptPresents(AssetExporter.AssemblyManager);
			}
			else
			{
				return false;
			}
		}

		public override bool Export(IExportContainer container, string projectDirectory)
		{
			Logger.Info(LogCategory.Export, "Exporting scripts...");

			string assetsDirectoryPath = Path.Combine(projectDirectory, AssetsKeyword);

			Dictionary<string, AssemblyDefinitionDetails> assemblyDefinitionDetailsDictionary = new();

			if (AssetExporter.AssemblyManager.IsSet)
			{
				string pluginsFolder = Path.Combine(assetsDirectoryPath, "Plugins");

				foreach (AssemblyDefinition assembly in AssetExporter.AssemblyManager.GetAssemblies())
				{
					string assemblyName = assembly.Name!;
					AssemblyExportType exportType = AssetExporter.GetExportType(assemblyName);

					if (exportType is AssemblyExportType.Decompile)
					{
						Logger.Info(LogCategory.Export, $"Decompiling {assemblyName}");
						string outputDirectory = Path.Combine(assetsDirectoryPath, GetScriptsFolderName(assemblyName), assemblyName);
						Directory.CreateDirectory(outputDirectory);
						AssetExporter.Decompiler.DecompileWholeProject(assembly, outputDirectory);

						assemblyDefinitionDetailsDictionary.TryAdd(assemblyName, new AssemblyDefinitionDetails(assembly, outputDirectory));
					}
					else if (exportType is AssemblyExportType.Save)
					{
						Logger.Info(LogCategory.Export, $"Saving {assemblyName}");
						Directory.CreateDirectory(pluginsFolder);
						string outputPath = Path.Combine(pluginsFolder, FilenameUtils.AddAssemblyFileExtension(assemblyName));
						AssetExporter.AssemblyManager.SaveAssembly(assembly, outputPath);
						OnAssemblyExported(container, outputPath);
					}
				}
			}

			foreach (IMonoScript asset in m_export)
			{
				GetExportSubPath(asset, out string subFolderPath, out string fileName);
				string folderPath = Path.Combine(assetsDirectoryPath, subFolderPath);
				string filePath = Path.Combine(folderPath, fileName);
				if (!System.IO.File.Exists(filePath))
				{
					Directory.CreateDirectory(folderPath);
					System.IO.File.WriteAllText(filePath, EmptyScript.GetContent(asset));
					string assemblyName = BaseManager.ToAssemblyName(asset.GetAssemblyNameFixed());
					if (!assemblyDefinitionDetailsDictionary.ContainsKey(assemblyName))
					{
						Debug.Assert(GetScriptsFolderName(assemblyName) is "Scripts");
						string assemblyDirectoryPath = Path.Combine(assetsDirectoryPath, "Scripts", assemblyName);
						AssemblyDefinitionDetails details = new AssemblyDefinitionDetails(assemblyName, assemblyDirectoryPath);
						assemblyDefinitionDetailsDictionary.Add(assemblyName, details);
					}
				}

				if (System.IO.File.Exists($"{filePath}.meta"))
				{
					Logger.Error(LogCategory.Export, $"Metafile already exists at {filePath}.meta");
					//throw new Exception($"Metafile already exists at {filePath}.meta");
				}
				else
				{
					OnScriptExported(container, asset, filePath);
				}
			}

			// assembly definitions were added in 2017.3
			//     see: https://blog.unity.com/technology/unity-2017-3b-feature-preview-assembly-definition-files-and-transform-tool
			if (assemblyDefinitionDetailsDictionary.Count > 0 && container.ExportVersion.IsGreaterEqual(2017, 3))
			{
				foreach (AssemblyDefinitionDetails details in assemblyDefinitionDetailsDictionary.Values)
				{
					// exclude predefined assemblies like Assembly-CSharp.dll
					//    see: https://docs.unity3d.com/2017.3/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html
					if (!ReferenceAssemblies.IsPredefinedAssembly(details.AssemblyName))
					{
						AssemblyDefinitionExporter.Export(details);
					}
				}
			}

			return true;
		}

		public override bool IsContains(IUnityObjectBase asset)
		{
			return m_scripts.ContainsKey(asset);
		}

		public override long GetExportID(IUnityObjectBase asset)
		{
			return ExportIdHandler.GetMainExportID(asset);
		}

		public override MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal)
		{
			if (isLocal)
			{
				throw new NotSupportedException();
			}

			IMonoScript script = m_scripts[asset];
			if (AssetExporter.GetExportType(script.GetAssemblyNameFixed()) is AssemblyExportType.Decompile)
			{
				long exportID = GetExportID(asset);
				UnityGUID uniqueGUID = ScriptHashing.ComputeScriptGuid(script);
				return new MetaPtr(exportID, uniqueGUID, AssetExporter.ToExportType(asset));
			}
			else
			{
				int fileID;
				UnityGUID guid;
				if (!script.HasAssemblyName() || ReferenceAssemblies.IsUnityEngineAssembly(script.GetAssemblyNameFixed()))
				{
					fileID = ScriptHashing.CalculateScriptFileID(script);
					guid = UnityEngineGUID;
				}
				else
				{
					string? scriptKey = $"{script.AssemblyName_C115}{script.Namespace_C115}{script.ClassName_C115}";
					if (!ScriptId.TryGetValue(scriptKey, out fileID))
					{
						fileID = ScriptHashing.CalculateScriptFileID(script);
						ScriptId.Add(scriptKey, fileID);
					}
					guid = GetAssemblyGuid(script.AssemblyName_C115);
				}

				return new MetaPtr(fileID, guid, AssetExporter.ToExportType(asset));
			}
		}

		private static void OnScriptExported(IExportContainer container, IUnityObjectBase asset, string path)
		{
			IMonoScript script = (IMonoScript)asset;
			IMonoImporter importer = MonoImporterFactory.CreateAsset(asset.Collection, container.ExportVersion);
			importer.ExecutionOrder_C1035 = (short)script.ExecutionOrder_C115;
			Meta meta = new Meta(ScriptHashing.ComputeScriptGuid(script), importer);
			ExportMeta(container, meta, path);
		}

		private void OnAssemblyExported(IExportContainer container, string path)
		{
			UnityGUID guid = GetAssemblyGuid(Path.GetFileName(path));
			IPluginImporter importer = PluginImporterFactory.CreateAsset(container.VirtualFile, container.ExportVersion);
			Meta meta = new Meta(guid, importer);
			ExportMeta(container, meta, path);
		}

		private static string GetScriptsFolderName(string assemblyName)
		{
			return assemblyName is "Assembly-CSharp-firstpass" or "Assembly - CSharp - firstpass" ? "Plugins" : "Scripts";
		}

		private static void GetExportSubPath(string assembly, string @namespace, string @class, out string folderPath, out string fileName)
		{
			string assemblyFolder = BaseManager.ToAssemblyName(assembly);
			string scriptsFolder = GetScriptsFolderName(assemblyFolder);
			string namespaceFolder = @namespace.Replace('.', Path.DirectorySeparatorChar);
			folderPath = DirectoryUtils.FixInvalidPathCharacters(Path.Combine(scriptsFolder, assemblyFolder, namespaceFolder));
			fileName = $"{DirectoryUtils.FixInvalidPathCharacters(@class)}.cs";
		}

		private static void GetExportSubPath(IMonoScript script, out string folderPath, out string fileName)
		{
			GetExportSubPath(script.GetAssemblyNameFixed(), script.Namespace_C115.String, script.ClassName_C115.String, out folderPath, out fileName);
		}

		private UnityGUID GetAssemblyGuid(string assemblyName)
		{
			if (AssemblyHash.TryGetValue(assemblyName, out UnityGUID result))
			{
				return result;
			}
			else
			{
				UnityGUID guid = ScriptHashing.CalculateAssemblyGuid(assemblyName);
				AssemblyHash[assemblyName] = guid;
				//MonoScripts don't always have the .dll extension.
				AssemblyHash[FilenameUtils.AddAssemblyFileExtension(assemblyName)] = guid;
				return guid;
			}
		}

		public override ScriptExporter AssetExporter { get; }
		public override AssetCollection File => throw new NotSupportedException();
		public override IEnumerable<IUnityObjectBase> Assets => m_scripts.Keys;
		public override string Name => nameof(ScriptExportCollection);

		private static readonly UnityGUID UnityEngineGUID = new UnityGUID(0x1F55507F, 0xA1948D44, 0x4080F528, 0xC176C90E);

		private readonly List<IMonoScript> m_export = new();
		private readonly Dictionary<IUnityObjectBase, IMonoScript> m_scripts = new();
		private readonly Dictionary<string, int> ScriptId = new();
		private readonly Dictionary<string, UnityGUID> AssemblyHash = new();
	}
}
