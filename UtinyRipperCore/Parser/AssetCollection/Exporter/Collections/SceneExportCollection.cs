using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;
using UtinyRipper.SerializedFiles;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public sealed class SceneExportCollection : ExportCollection, IComparer<Object>
	{
		public SceneExportCollection(IAssetExporter assetExporter, ISerializedFile file)
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

			foreach(Object asset in file.FetchAssets())
			{
				if(OcclusionCullingSettings.IsCompatible(asset))
				{
					AddComponent(file, asset);
				}
			}
			m_cexportIDs = m_cexportIDs.OrderBy(t => t.Key, this).ToDictionary(t => t.Key, t => t.Value);

			if(OcclusionCullingSettings.IsReadPVSData(file.Version))
			{
				if (Config.IsGenerateGUIDByContent)
				{
					GUID = ObjectUtils.CalculateObjectsGUID(Assets);
				}
				else
				{
					GUID = new EngineGUID(Guid.NewGuid());
				}
			}
			else
			{
				OcclusionCullingSettings sceneSettings = Components.Where(t => t.ClassID == ClassIDType.OcclusionCullingSettings).Select(t => (OcclusionCullingSettings)t).FirstOrDefault();
				if(sceneSettings == null)
				{
					GUID = new EngineGUID(Guid.NewGuid());
				}
				else
				{
					GUID = sceneSettings.SceneGUID;
				}
			}
		}

		private static bool IsReadMainData(Version version)
		{
			return version.IsLess(5);
		}

		public override bool Export(ProjectAssetContainer container, string dirPath)
		{
			TryInitialize(container);

			string folderPath = Path.Combine(dirPath, Object.AssetsKeyWord, OcclusionCullingSettings.SceneKeyWord);
			string sceneSubPath = GetSceneName(container);
			string fileName = $"{sceneSubPath}.unity";
			string filePath = Path.Combine(folderPath, fileName);
			folderPath = Path.GetDirectoryName(filePath);

			if (!DirectoryUtils.Exists(folderPath))
			{
				DirectoryUtils.CreateDirectory(folderPath);
			}

			AssetExporter.Export(container, Components, filePath);
			SceneImporter sceneImporter = new SceneImporter();
			Meta meta = new Meta(sceneImporter, GUID);
			ExportMeta(container, meta, filePath);

			string sceneName = Path.GetFileName(sceneSubPath);
			string subFolderPath = Path.Combine(folderPath, sceneName);
			if (OcclusionCullingData != null)
			{
				ExportAsset(container, OcclusionCullingData, subFolderPath);
			}

			return true;
		}

		public override bool IsContains(Object asset)
		{
			if(asset == OcclusionCullingData)
			{
				return true;
			}
			return m_cexportIDs.ContainsKey(asset);
		}

		public override ulong GetExportID(Object asset)
		{
			return IsComponent(asset) ? m_cexportIDs[asset] : GetMainExportID(asset);
		}
		
		public override ExportPointer CreateExportPointer(Object asset, bool isLocal)
		{
			ulong exportID = GetExportID(asset);
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
				if(obj2.ClassID.IsSceneSettings())
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

		private void TryInitialize(ProjectAssetContainer container)
		{
			if(m_initialized)
			{
				return;
			}

			foreach(Object comp in Components)
			{
				if(comp.ClassID == ClassIDType.OcclusionCullingSettings)
				{
					OcclusionCullingSettings settings = (OcclusionCullingSettings)comp;
					if (OcclusionCullingSettings.IsReadPVSData(File.Version))
					{
						if (settings.PVSData.Count > 0)
						{
							OcclusionCullingData = new OcclusionCullingData(container.VirtualFile);
							OcclusionCullingData.Initialize(container, settings);
						}
					}
				}
			}

			m_initialized = true;
		}

		private void AddComponent(ISerializedFile file, Object comp)
		{
			m_cexportIDs.Add(comp, unchecked((ulong)comp.PathID));
		}

		private bool IsComponent(Object asset)
		{
			return asset != OcclusionCullingData;
		}

		private string GetSceneName(IExportContainer container)
		{
			if(Name == MainSceneName || m_sceneNameFormat.IsMatch(Name))
			{
				int index = GetSceneIndex(Name, File.Version);
				string scenePath = container.SceneIDToString(index);
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

		private static int GetSceneIndex(string name, Version version)
		{
			if (IsReadMainData(version))
			{
				if (name == MainSceneName)
				{
					return 0;
				}
				else
				{
					string indexStr = name.Substring(LevelName.Length);
					return int.Parse(indexStr) + 1;
				}
			}
			else
			{
				string indexStr = name.Substring(LevelName.Length);
				return int.Parse(indexStr);
			}
		}

		public override IAssetExporter AssetExporter { get; }
		public override IEnumerable<Object> Assets
		{
			get
			{
				foreach(Object asset in Components)
				{
					yield return asset;
				}
				if(OcclusionCullingData != null)
				{
					yield return OcclusionCullingData;
				}
			}
		}
		public override string Name { get; }
		public override ISerializedFile File => m_file;

		public OcclusionCullingData OcclusionCullingData { get; private set; }
		public EngineGUID GUID { get; }

		private IEnumerable<Object> Components => m_cexportIDs.Keys;

		private const string AssetsName = "Assets/";
		private const string LevelName = "level";
		private const string MainSceneName = "maindata";

		private static readonly Regex m_sceneNameFormat = new Regex($"{LevelName}[0-9]+");

		private readonly Dictionary<Object, ulong> m_cexportIDs = new Dictionary<Object, ulong>();
		private readonly ISerializedFile m_file;

		private bool m_initialized = false;
	}
}
