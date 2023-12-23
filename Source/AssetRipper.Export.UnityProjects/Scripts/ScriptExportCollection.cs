using AsmResolver.DotNet;
using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects.Scripts.AssemblyDefinitions;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files.Utils;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Classes.ClassID_1035;
using AssetRipper.SourceGenerated.Classes.ClassID_1050;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Subclasses.PlatformSettingsData_Plugin;
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
					string assemblyName = asset.GetAssemblyNameFixed();
					if (!assemblyDefinitionDetailsDictionary.ContainsKey(assemblyName))
					{
						string assemblyDirectoryPath = Path.Combine(assetsDirectoryPath, GetScriptsFolderName(assemblyName), assemblyName);
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
			if (assemblyDefinitionDetailsDictionary.Count > 0 && container.ExportVersion.GreaterThanOrEquals(2017, 3))
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
				UnityGuid uniqueGUID = ScriptHashing.ComputeScriptGuid(script);
				return new MetaPtr(exportID, uniqueGUID, AssetExporter.ToExportType(asset));
			}
			else
			{
				int fileID;
				UnityGuid guid;
				if (!script.HasAssemblyName() || ReferenceAssemblies.IsUnityEngineAssembly(script.GetAssemblyNameFixed()))
				{
					fileID = ScriptHashing.CalculateScriptFileID(script);
					guid = UnityEngineGUID;
				}
				else
				{
					string? scriptKey = $"{script.AssemblyName}{script.Namespace}{script.ClassName_R}";
					if (!ScriptId.TryGetValue(scriptKey, out fileID))
					{
						fileID = ScriptHashing.CalculateScriptFileID(script);
						ScriptId.Add(scriptKey, fileID);
					}
					guid = GetAssemblyGuid(script.AssemblyName);
				}

				return new MetaPtr(fileID, guid, AssetExporter.ToExportType(asset));
			}
		}

		private static void OnScriptExported(IExportContainer container, IUnityObjectBase asset, string path)
		{
			IMonoScript script = (IMonoScript)asset;
			IMonoImporter importer = MonoImporter.Create(asset.Collection, container.ExportVersion);
			importer.ExecutionOrder = (short)script.ExecutionOrder;
			Meta meta = new Meta(ScriptHashing.ComputeScriptGuid(script), importer);
			ExportMeta(container, meta, path);
		}

		private void OnAssemblyExported(IExportContainer container, string path)
		{
			Span<bool> test = stackalloc bool[1];
			UnityGuid guid = GetAssemblyGuid(Path.GetFileName(path));
			IPluginImporter importer = PluginImporter.Create(container.VirtualFile, container.ExportVersion);
			if (HasPlatformData(importer))
			{
				PlatformSettingsData_Plugin anyPlatformSettings = AddPlatformSettings(importer, "Any", Utf8String.Empty);
				anyPlatformSettings.Enabled = true;

				PlatformSettingsData_Plugin editorPlatformSettings = AddPlatformSettings(importer, "Editor", "Editor");
				editorPlatformSettings.Enabled = false;
				editorPlatformSettings.Settings.Add("DefaultValueInitialized", "true");
			}
			
			Meta meta = new Meta(guid, importer);
			ExportMeta(container, meta, path);

			static bool HasPlatformData(IPluginImporter importer)
			{
				return importer.Has_PlatformData_AssetDictionary_AssetPair_Utf8String_Utf8String_PlatformSettingsData_Plugin()
					|| importer.Has_PlatformData_AssetDictionary_Utf8String_PlatformSettingsData_Plugin();
			}

			static PlatformSettingsData_Plugin AddPlatformSettings(IPluginImporter importer, Utf8String platformKey, Utf8String platformValue)
			{
				if (importer.Has_PlatformData_AssetDictionary_AssetPair_Utf8String_Utf8String_PlatformSettingsData_Plugin())
				{
					(AssetPair<Utf8String, Utf8String> pair, PlatformSettingsData_Plugin data)
						= importer.PlatformData_AssetDictionary_AssetPair_Utf8String_Utf8String_PlatformSettingsData_Plugin.AddNew();
					pair.Key = platformKey;
					pair.Value = platformValue;
					return data;
				}
				else if (importer.Has_PlatformData_AssetDictionary_Utf8String_PlatformSettingsData_Plugin())
				{
					AssetPair<Utf8String, PlatformSettingsData_Plugin> pair
						= importer.PlatformData_AssetDictionary_Utf8String_PlatformSettingsData_Plugin.AddNew();

					pair.Key = platformKey;
					return pair.Value;
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
		}

		private static string GetScriptsFolderName(string assemblyName)
		{
			return assemblyName is "Assembly-CSharp-firstpass" or "Assembly - CSharp - firstpass" ? "Plugins" : "Scripts";
		}

		private static void GetExportSubPath(string assembly, string @namespace, string @class, out string folderPath, out string fileName)
		{
			string assemblyFolder = FilenameUtils.RemoveAssemblyFileExtension(assembly);
			string scriptsFolder = GetScriptsFolderName(assemblyFolder);
			string namespaceFolder = @namespace.Replace('.', Path.DirectorySeparatorChar);
			folderPath = DirectoryUtils.FixInvalidPathCharacters(Path.Combine(scriptsFolder, assemblyFolder, namespaceFolder));
			fileName = $"{DirectoryUtils.FixInvalidPathCharacters(@class)}.cs";
		}

		private static void GetExportSubPath(IMonoScript script, out string folderPath, out string fileName)
		{
			GetExportSubPath(script.GetAssemblyNameFixed(), script.Namespace.String, script.ClassName_R.String, out folderPath, out fileName);
		}

		private UnityGuid GetAssemblyGuid(string assemblyName)
		{
			if (AssemblyHash.TryGetValue(assemblyName, out UnityGuid result))
			{
				return result;
			}
			else
			{
				UnityGuid guid = ScriptHashing.CalculateAssemblyGuid(assemblyName);
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

		private static readonly UnityGuid UnityEngineGUID = new UnityGuid(0x1F55507F, 0xA1948D44, 0x4080F528, 0xC176C90E);

		private readonly List<IMonoScript> m_export = new();
		private readonly Dictionary<IUnityObjectBase, IMonoScript> m_scripts = new();
		private readonly Dictionary<string, int> ScriptId = new();
		private readonly Dictionary<string, UnityGuid> AssemblyHash = new();
	}
}
