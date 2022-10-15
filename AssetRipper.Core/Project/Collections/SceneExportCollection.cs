using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated.Classes.ClassID_1030;
using AssetRipper.SourceGenerated.Classes.ClassID_1034;
using AssetRipper.SourceGenerated.Classes.ClassID_29;
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using AssetRipper.SourceGenerated.Classes.ClassID_363;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace AssetRipper.Core.Project.Collections
{
	public class SceneExportCollection : ExportCollection, IComparer<IUnityObjectBase>
	{
		public SceneExportCollection(IAssetExporter assetExporter, AssetCollection file)
		{
			AssetExporter = assetExporter ?? throw new ArgumentNullException(nameof(assetExporter));
			File = file ?? throw new ArgumentNullException(nameof(file));

			List<IUnityObjectBase> components = new();
			foreach (IUnityObjectBase asset in file)
			{
				if (SceneExportHelpers.IsSceneCompatible(asset))
				{
					components.Add(asset);
					m_exportIDs.Add(asset.AssetInfo, asset.PathID);
				}
			}
			m_components = components.OrderBy(t => t, this).ToArray();

			foreach (IUnityObjectBase comp in Components)
			{
				if (comp is IOcclusionCullingSettings settings)
				{
					if (settings.PVSData_C29?.Length > 0)
					{
						m_occlusionCullingSettings = settings;
						break;
					}
				}
			}
		}

		public override bool Export(IProjectAssetContainer container, string projectDirectory)
		{
			string sceneSubPath = SceneExportHelpers.GetSceneSubPath(container, File, out bool isRegular);
			string filePath = isRegular
				? Path.Combine(projectDirectory, AssetsKeyword, $"{sceneSubPath}.{ExportExtension}")
				: Path.Combine(projectDirectory, AssetsKeyword, "Scenes", $"{sceneSubPath}.{ExportExtension}");
			string folderPath = Path.GetDirectoryName(filePath)!;
			string sceneName = Path.GetFileName(sceneSubPath);

			if (SceneExportHelpers.IsDuplicate(container, File))
			{
				if (System.IO.File.Exists(filePath))
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"Duplicate scene '{sceneSubPath}' has been found. Skipping");
					return false;
				}
			}

			Directory.CreateDirectory(folderPath);
			return ExportScene(container, folderPath, filePath, sceneName);
		}

		protected virtual bool ExportScene(IProjectAssetContainer container, string folderPath, string filePath, string sceneName)
		{
			AssetExporter.Export(container, Components, filePath);
			IDefaultImporter sceneImporter = DefaultImporterFactory.CreateAsset(container.ExportVersion);
			Meta meta = new Meta(GUID, sceneImporter);
			ExportMeta(container, meta, filePath);

			string subFolderPath = Path.Combine(folderPath, sceneName);
			if (OcclusionCullingData is not null && m_occlusionCullingSettings is not null)
			{
				OcclusionCullingData.Initialize(container, m_occlusionCullingSettings);
				ExportAsset(container, OcclusionCullingData, subFolderPath);
			}

			return true;
		}

		public override bool IsContains(IUnityObjectBase asset)
		{
			if (asset == OcclusionCullingData)
			{
				return true;
			}
			return m_exportIDs.ContainsKey(asset.AssetInfo);
		}

		public override long GetExportID(IUnityObjectBase asset)
		{
			return IsComponent(asset) ? m_exportIDs[asset.AssetInfo] : ExportIdHandler.GetMainExportID(asset);
		}

		public override MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal)
		{
			long exportID = GetExportID(asset);
			if (isLocal && IsComponent(asset))
			{
				return new MetaPtr(exportID);
			}
			else
			{
				UnityGUID guid = IsComponent(asset) ? GUID : asset.GUID;
				return new MetaPtr(exportID, guid, AssetType.Serialized);
			}
		}

		public int Compare(IUnityObjectBase? obj1, IUnityObjectBase? obj2)
		{
			if (obj1?.ClassID == obj2?.ClassID)
			{
				return 0;
			}

			if (obj1 is ILevelGameManager)
			{
				if (obj2 is ILevelGameManager)
				{
					return obj1.ClassID < obj2.ClassID ? -1 : 1;
				}
				else
				{
					return -1;
				}
			}
			else if (obj2 is ILevelGameManager)
			{
				return 1;
			}
			else
			{
				return 0;
			}
		}

		private void ExportAsset(IProjectAssetContainer container, IHasNameString asset, string path)
		{
			INativeFormatImporter importer = NativeFormatImporterFactory.CreateAsset(container.ExportVersion);
			importer.MainObjectFileID_C1034 = GetExportID((IUnityObjectBase)asset);
			ExportAsset(container, importer, (IUnityObjectBase)asset, path, asset.NameString);
		}

		private bool IsComponent(IUnityObjectBase asset)
		{
			return asset != OcclusionCullingData;
		}

		public override IEnumerable<IUnityObjectBase> Assets
		{
			get
			{
				foreach (IUnityObjectBase asset in Components)
				{
					yield return asset;
				}
				if (OcclusionCullingData != null)
				{
					yield return OcclusionCullingData;
				}
			}
		}

		public virtual string ExportExtension => "unity";
		public override string Name => File.Name;
		public override AssetCollection File { get; }
		public IOcclusionCullingData? OcclusionCullingData { get; }
		public UnityGUID GUID => File.GUID;
		private IEnumerable<IUnityObjectBase> Components => m_components;
		public override IAssetExporter AssetExporter { get; }

		private readonly IUnityObjectBase[] m_components;
		private readonly Dictionary<AssetInfo, long> m_exportIDs = new();
		private readonly IOcclusionCullingSettings? m_occlusionCullingSettings;
	}
}
