using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

			foreach(Object asset in file.FetchAssets())
			{
				if(asset.IsComponent || asset.ClassID == ClassIDType.GameObject)
				{
					AddComponent(file, asset);
				}
			}
			m_cexportIDs = m_cexportIDs.OrderBy(t => t.Key, this).ToDictionary(t => t.Key, t => t.Value);

			if(OcclusionCullingSettings.IsReadPVSData(file.Version))
			{
				if (Config.IsGenerateGUIDByContent)
				{
					GUID = ObjectUtils.CalculateObjectsGUID(Objects);
				}
				else
				{
					GUID = new UtinyGUID(Guid.NewGuid());
				}
			}
			else
			{
				GUID = Components.Where(t => t.ClassID == ClassIDType.OcclusionCullingSettings).Select(t => (OcclusionCullingSettings)t).First().SceneGUID;
			}
		}

		public override bool Export(ProjectAssetContainer container, string dirPath)
		{
			string folderPath = Path.Combine(dirPath, OcclusionCullingSettings.SceneExportFolder);
			string fileName = $"{Name}.unity";
			string filePath = Path.Combine(folderPath, fileName);

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			AssetExporter.Export(container, Components, filePath);
			DefaultImporter sceneImporter = new DefaultImporter(false);
			Meta meta = new Meta(sceneImporter, GUID);
			ExportMeta(container, meta, filePath);

			string subFolderPath = Path.Combine(folderPath, Name);
			if (OcclusionCullingData != null)
			{
				ExportAsset(container, OcclusionCullingData, subFolderPath);
			}
			if (LightingDataAsset != null)
			{
				ExportAsset(container, OcclusionCullingData, subFolderPath);
			}
			if (m_navMeshData != null)
			{
				ExportAsset(container, m_navMeshData, subFolderPath);
			}

			return true;
		}

		public override bool IsContains(Object asset)
		{
			if(asset == OcclusionCullingData)
			{
				return true;
			}
			if(asset == LightingDataAsset)
			{
				return true;
			}
			if(asset == m_navMeshData)
			{
				return true;
			}
			return m_cexportIDs.ContainsKey(asset);
		}

		public override string GetExportID(Object asset)
		{
			return IsComponent(asset) ? m_cexportIDs[asset] : GetMainExportID(asset);
		}
		
		public override ExportPointer CreateExportPointer(Object asset, bool isLocal)
		{
			string exportID = GetExportID(asset);
			if (isLocal && IsComponent(asset))
			{
				return new ExportPointer(exportID);
			}
			else
			{
				UtinyGUID guid = IsComponent(asset) ? GUID : asset.GUID;
				return new ExportPointer(exportID, guid, AssetType.Serialized);
			}				
		}

		public int Compare(Object obj1, Object obj2)
		{
			if (obj1.ClassID == obj2.ClassID)
			{
				return 0;
			}

			if (obj1.ClassID.IsSceneComponent())
			{
				if(obj2.ClassID.IsSceneComponent())
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
				if (obj2.ClassID.IsSceneComponent())
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

		private void AddComponent(ISerializedFile file, Object comp)
		{
			if(comp.ClassID == ClassIDType.NavMeshSettings)
			{
				NavMeshSettings settings = (NavMeshSettings)comp;
				NavMeshData data = settings.NavMeshData.FindObject(file);
				if (data != null)
				{
					m_navMeshData = data;
				}
			}

			m_cexportIDs.Add(comp, comp.PathID.ToString());
		}

		private bool IsComponent(Object asset)
		{
			return asset != OcclusionCullingData && asset != LightingDataAsset && asset != m_navMeshData;
		}

		public override IAssetExporter AssetExporter { get; }
		public override IEnumerable<Object> Objects
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
				if (LightingDataAsset != null)
				{
					yield return LightingDataAsset;
				}
				if (m_navMeshData != null)
				{
					yield return m_navMeshData;
				}
			}
		}
		public override string Name { get; }

		public OcclusionCullingData OcclusionCullingData { get; set; }
		public LightingDataAsset LightingDataAsset { get; set; }
		public UtinyGUID GUID { get; }

		private IEnumerable<Object> Components => m_cexportIDs.Keys;

		private readonly Dictionary<Object, string> m_cexportIDs = new Dictionary<Object, string>();

		private NavMeshData m_navMeshData;
	}
}
