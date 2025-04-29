using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Logging;
using AssetRipper.Processing.Prefabs;
using AssetRipper.Processing.Scenes;
using AssetRipper.SourceGenerated.Classes.ClassID_1030;
using AssetRipper.SourceGenerated.Classes.ClassID_3;

namespace AssetRipper.Export.UnityProjects.Project
{
	public class SceneExportCollection : ExportCollection, IComparer<IUnityObjectBase>
	{
		public SceneExportCollection(IAssetExporter assetExporter, SceneHierarchyObject hierarchy)
		{
			ArgumentNullException.ThrowIfNull(assetExporter);
			ArgumentNullException.ThrowIfNull(hierarchy);

			AssetExporter = assetExporter;
			Hierarchy = hierarchy;
			CurrentFile = Hierarchy.Collection;//Have to set it to something.

			foreach (IUnityObjectBase asset in Hierarchy.Assets)
			{
				m_exportIDs.Add(asset, asset.Collection is SerializedAssetCollection ? asset.PathID : ExportIdHandler.GetInternalId());
			}

			componentArray = hierarchy.ExportableAssets.Order(this).ToArray();
		}

		public override bool Export(IExportContainer container, string projectDirectory, FileSystem fileSystem)
		{
			string filePath = fileSystem.Path.Join(projectDirectory, $"{Scene.Path}.{ExportExtension}");
			string folderPath = fileSystem.Path.GetDirectoryName(filePath)!;

			if (IsSceneDuplicate(container))
			{
				if (fileSystem.File.Exists(filePath))
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"Duplicate scene '{Scene.Path}' has been found. Skipping");
					return false;
				}
			}

			fileSystem.Directory.Create(folderPath);
			return ExportScene(container, folderPath, filePath, Scene.Name, fileSystem);
		}

		protected virtual bool ExportScene(IExportContainer container, string folderPath, string filePath, string sceneName, FileSystem fileSystem)
		{
			AssetExporter.Export(container, ExportableAssets, filePath, fileSystem);
			IDefaultImporter sceneImporter = DefaultImporter.Create(container.File, container.ExportVersion);
			if (sceneImporter.Has_AssetBundleName_R() && Hierarchy.AssetBundleName is not null)
			{
				sceneImporter.AssetBundleName_R = Hierarchy.AssetBundleName;
			}
			Meta meta = new Meta(GUID, sceneImporter);
			ExportMeta(container, meta, filePath, fileSystem);
			return true;
		}

		public override bool Contains(IUnityObjectBase asset)
		{
			return m_exportIDs.ContainsKey(asset);
		}

		public override long GetExportID(IExportContainer container, IUnityObjectBase asset)
		{
			return m_exportIDs[asset];
		}

		public override MetaPtr CreateExportPointer(IExportContainer container, IUnityObjectBase asset, bool isLocal)
		{
			long exportID = GetExportID(container, asset);
			if (isLocal)
			{
				return new MetaPtr(exportID);
			}
			else
			{
				return new MetaPtr(exportID, GUID, AssetType.Serialized);
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

		private bool IsSceneDuplicate(IExportContainer container)
		{
			if (SceneHelpers.IsSceneName(File.Name))
			{
				int index = SceneHelpers.FileNameToSceneIndex(File.Name, File.Version);
				return container.IsSceneDuplicate(index);
			}
			return false;
		}

		public override IEnumerable<IUnityObjectBase> Assets
		{
			get
			{
				foreach (IUnityObjectBase asset in Hierarchy.Assets)
				{
					CurrentFile = asset.Collection;
					yield return asset;
				}
				CurrentFile = Hierarchy.Collection;
				yield return Hierarchy;
			}
		}

		public override IEnumerable<IUnityObjectBase> ExportableAssets
		{
			get
			{
				foreach (IUnityObjectBase asset in componentArray)
				{
					CurrentFile = asset.Collection;
					yield return asset;
				}
			}
		}

		public virtual string ExportExtension => "unity";

		/// <summary>
		/// The <see cref="SceneDefinition.Name"/> of <see cref="Scene"/>.
		/// </summary>
		public override string Name => Scene.Name;

		public override AssetCollection File => CurrentFile;
		public override UnityGuid GUID => Scene.GUID;
		public override IAssetExporter AssetExporter { get; }
		public SceneHierarchyObject Hierarchy { get; }
		public SceneDefinition Scene => Hierarchy.Scene;
		private AssetCollection CurrentFile { get; set; }

		private readonly IUnityObjectBase[] componentArray;
		private readonly Dictionary<IUnityObjectBase, long> m_exportIDs = new();
	}
}
