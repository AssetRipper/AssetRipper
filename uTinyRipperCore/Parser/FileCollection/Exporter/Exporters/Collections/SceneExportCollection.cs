using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using uTinyRipper.AssetExporters.Classes;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.AssetExporters
{
	public sealed class SceneExportCollection : ExportCollection, IComparer<Object>
	{
		public SceneExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, ISerializedFile file)
		{
			if (assetExporter == null)
			{
				throw new ArgumentNullException(nameof(assetExporter));
			}
			if (file == null)
			{
				throw new ArgumentNullException(nameof(file));
			}

			AssetExporter = assetExporter;
			Name = file.Name;
			m_file = file;

			foreach (Object asset in file.FetchAssets())
			{
				if (OcclusionCullingSettings.IsSceneCompatible(asset))
				{
					m_cexportIDs.Add(asset, asset.PathID);
				}
			}
			m_cexportIDs = m_cexportIDs.OrderBy(t => t.Key, this).ToDictionary(t => t.Key, t => t.Value);

			if (OcclusionCullingSettings.IsReadSceneGUID(file.Version))
			{
				OcclusionCullingSettings sceneSettings = Components.Where(t => t.ClassID == ClassIDType.OcclusionCullingSettings).Select(t => (OcclusionCullingSettings)t).FirstOrDefault();
				if (sceneSettings != null)
				{
					GUID = sceneSettings.SceneGUID;
				}
			}
			if (GUID.IsZero)
			{
				if (Config.IsGenerateGUIDByContent)
				{
					GUID = ObjectUtils.CalculateAssetsGUID(Assets);
				}
				else
				{
					GUID = new EngineGUID(Guid.NewGuid());
				}
			}

			if (OcclusionCullingSettings.IsReadPVSData(File.Version))
			{
				foreach (Object comp in Components)
				{
					if (comp.ClassID == ClassIDType.OcclusionCullingSettings)
					{
						OcclusionCullingSettings settings = (OcclusionCullingSettings)comp;
						if (settings.PVSData.Count > 0)
						{
							m_occlusionCullingSettings = settings;
							OcclusionCullingData = OcclusionCullingData.CreateVirtualInstance(virtualFile, settings);
							break;
						}
					}
				}
			}
		}

		public static bool IsReadMainData(Version version)
		{
			return version.IsLess(5, 3);
		}

		public static string SceneIndexToFileName(int index, Version version)
		{
			if (IsReadMainData(version))
			{
				if (index == 0)
				{
					return MainSceneName;
				}
				return LevelName + (index - 1).ToString();
			}
			return LevelName + index.ToString();
		}

		public static int FileNameToSceneIndex(string name, Version version)
		{
			if (IsReadMainData(version))
			{
				if (name == MainSceneName)
				{
					return 0;
				}

				string indexStr = name.Substring(LevelName.Length);
				return int.Parse(indexStr) + 1;
			}
			else
			{
				string indexStr = name.Substring(LevelName.Length);
				return int.Parse(indexStr);
			}
		}

		public override bool Export(ProjectAssetContainer container, string dirPath)
		{
			string folderPath = Path.Combine(dirPath, Object.AssetsKeyword, OcclusionCullingSettings.SceneKeyword);
			string sceneSubPath = GetSceneName(container);
			string fileName = $"{sceneSubPath}.unity";
			string filePath = Path.Combine(folderPath, fileName);

			if (IsDuplicate(container))
			{
				if (FileUtils.Exists(filePath))
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"Duplicate scene '{sceneSubPath}' has been found. Skipping");
					return false;
				}
			}

			folderPath = Path.GetDirectoryName(filePath);
			if (!DirectoryUtils.Exists(folderPath))
			{
				DirectoryUtils.CreateVirtualDirectory(folderPath);
			}

			AssetExporter.Export(container, Components, filePath);
			SceneImporter sceneImporter = new SceneImporter();
			Meta meta = new Meta(sceneImporter, GUID);
			ExportMeta(container, meta, filePath);

			string sceneName = Path.GetFileName(sceneSubPath);
			string subFolderPath = Path.Combine(folderPath, sceneName);
			if (OcclusionCullingData != null)
			{
				OcclusionCullingData.Initialize(container, m_occlusionCullingSettings);
				ExportAsset(container, OcclusionCullingData, subFolderPath);
			}

			return true;
		}

		public override bool IsContains(Object asset)
		{
			if (asset == OcclusionCullingData)
			{
				return true;
			}
			return m_cexportIDs.ContainsKey(asset);
		}

		public override long GetExportID(Object asset)
		{
			return IsComponent(asset) ? m_cexportIDs[asset] : GetMainExportID(asset);
		}
		
		public override ExportPointer CreateExportPointer(Object asset, bool isLocal)
		{
			long exportID = GetExportID(asset);
			if (isLocal && IsComponent(asset))
			{
				return new ExportPointer(exportID);
			}
			else
			{
				EngineGUID guid = IsComponent(asset) ? GUID : asset.GUID;
				return new ExportPointer(exportID, guid, AssetType.Serialized);
			}				
		}

		public int Compare(Object obj1, Object obj2)
		{
			if (obj1.ClassID == obj2.ClassID)
			{
				return 0;
			}

			if (obj1.ClassID.IsSceneSettings())
			{
				if (obj2.ClassID.IsSceneSettings())
				{
					return obj1.ClassID < obj2.ClassID ? -1 : 1;
				}
				else
				{
					return -1;
				}
			}
			else
			{
				if (obj2.ClassID.IsSceneSettings())
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
		}

		private void ExportAsset(ProjectAssetContainer container, NamedObject asset, string path)
		{
			NativeFormatImporter importer = new NativeFormatImporter(asset);
			ExportAsset(container, importer, asset, path, asset.Name);
		}

		private bool IsComponent(Object asset)
		{
			return asset != OcclusionCullingData;
		}

		private string GetSceneName(IExportContainer container)
		{
			if (IsSceneName)
			{
				int index = FileNameToSceneIndex(Name, File.Version);
				string scenePath = container.SceneIndexToName(index);
				if (scenePath.StartsWith(AssetsName, StringComparison.Ordinal))
				{
					string relativePath = scenePath.Substring(AssetsName.Length);
					string extension = Path.GetExtension(scenePath);
					return relativePath.Substring(0, relativePath.Length - extension.Length);
				}
				else
				{
					return scenePath;
				}
			}
			return Name;
		}

		private bool IsDuplicate(IExportContainer container)
		{
			if (IsSceneName)
			{
				int index = FileNameToSceneIndex(Name, File.Version);
				return container.IsSceneDuplicate(index);
			}
			return false;
		}

		private bool IsSceneName => Name == MainSceneName || s_sceneNameFormat.IsMatch(Name);

		public override IAssetExporter AssetExporter { get; }
		public override IEnumerable<Object> Assets
		{
			get
			{
				foreach (Object asset in Components)
				{
					yield return asset;
				}
				if (OcclusionCullingData != null)
				{
					yield return OcclusionCullingData;
				}
			}
		}
		public override string Name { get; }
		public override ISerializedFile File => m_file;

		public OcclusionCullingData OcclusionCullingData { get; }
		public EngineGUID GUID { get; }

		private IEnumerable<Object> Components => m_cexportIDs.Keys;

		private const string AssetsName = "Assets/";
		private const string LevelName = "level";
		private const string MainSceneName = "maindata";

		private static readonly Regex s_sceneNameFormat = new Regex($"^{LevelName}(0|[1-9][0-9]*)$");

		private readonly Dictionary<Object, long> m_cexportIDs = new Dictionary<Object, long>();
		private readonly ISerializedFile m_file;
		private readonly OcclusionCullingSettings m_occlusionCullingSettings;
	}
}
