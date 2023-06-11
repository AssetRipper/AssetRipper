using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Import.Configuration;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Primitives;
using AssetRipper.Processing.Scenes;
using AssetRipper.SourceGenerated.Classes.ClassID_141;
using AssetRipper.SourceGenerated.Classes.ClassID_78;
using AssetRipper.SourceGenerated.Extensions;


namespace AssetRipper.Export.UnityProjects.Project
{
	public class ProjectAssetContainer : IExportContainer
	{
		public ProjectAssetContainer(ProjectExporter exporter, CoreConfiguration options, TemporaryAssetCollection file, IEnumerable<IUnityObjectBase> assets,
			IReadOnlyList<IExportCollection> collections)
		{
			m_exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
			VirtualFile = file ?? throw new ArgumentNullException(nameof(file));

			foreach (IUnityObjectBase asset in assets)
			{
				if (asset is IBuildSettings buildSettings)
				{
					m_buildSettings = buildSettings;
				}
				else if (asset is ITagManager tagManager)
				{
					m_tagManager = tagManager;
				}
			}

			List<SceneExportCollection> scenes = new List<SceneExportCollection>();
			foreach (IExportCollection collection in collections)
			{
				foreach (IUnityObjectBase asset in collection.Assets)
				{
#warning TODO: unique asset:collection (m_assetCollections.Add)
					m_assetCollections[asset.AssetInfo] = collection;
				}
				if (collection is SceneExportCollection scene)
				{
					scenes.Add(scene);
				}
			}
			m_scenes = scenes.ToArray();
		}

		public IUnityObjectBase? TryGetAsset(long pathID)
		{
			return File.TryGetAsset(pathID);
		}

		public IUnityObjectBase GetAsset(long pathID)
		{
			return File.GetAsset(pathID);
		}

		public virtual IUnityObjectBase? TryGetAsset(int fileIndex, long pathID)
		{
			return File.TryGetAsset(fileIndex, pathID);
		}

		public virtual IUnityObjectBase GetAsset(int fileIndex, long pathID)
		{
			return File.GetAsset(fileIndex, pathID);
		}

		public long GetExportID(IUnityObjectBase asset)
		{
			if (m_assetCollections.TryGetValue(asset.AssetInfo, out IExportCollection? collection))
			{
				return collection.GetExportID(asset);
			}

			return ExportIdHandler.GetMainExportID(asset);
		}

		public AssetType ToExportType(Type type)
		{
			return m_exporter.ToExportType(type);
		}

		public MetaPtr CreateExportPointer(IUnityObjectBase asset)
		{
			if (m_assetCollections.TryGetValue(asset.AssetInfo, out IExportCollection? collection))
			{
				return collection.CreateExportPointer(asset, collection == CurrentCollection);
			}

			return MetaPtr.CreateMissingReference(asset.ClassID, AssetType.Meta);
		}

		public UnityGUID SceneNameToGUID(string name)
		{
			if (m_buildSettings == null)
			{
				return default;
			}

			int index = m_buildSettings.Scenes_C141.IndexOf(s => s.String == name);
			if (index == -1)
			{
				throw new Exception($"Scene '{name}' hasn't been found in build settings");
			}

			string fileName = SceneHelpers.SceneIndexToFileName(index, Version);
			foreach (SceneExportCollection scene in m_scenes)
			{
				if (scene.FileName == fileName)
				{
					return scene.GUID;
				}
			}
			return default;
		}

		public string SceneIndexToName(int sceneIndex)
		{
			return m_buildSettings == null ? $"level{sceneIndex}" : m_buildSettings.Scenes_C141[sceneIndex].String;
		}

		public bool IsSceneDuplicate(int sceneIndex) => SceneHelpers.IsSceneDuplicate(sceneIndex, m_buildSettings);

		public string TagIDToName(int tagID)
		{
			return m_tagManager.TagIDToName(tagID);
		}

		public ushort TagNameToID(string tagName)
		{
			return m_tagManager.TagNameToID(tagName);
		}

		public IExportCollection CurrentCollection { get; set; }
		public TemporaryAssetCollection VirtualFile { get; }
		public virtual AssetCollection File => CurrentCollection.File;
		public string Name => File.Name;
		public UnityVersion Version => File.Version;
		public BuildTarget Platform => File.Platform;
		public TransferInstructionFlags Flags => File.Flags;
		public UnityVersion ExportVersion => VirtualFile.Version;
		public BuildTarget ExportPlatform => VirtualFile.Platform;
		public virtual TransferInstructionFlags ExportFlags => VirtualFile.Flags | CurrentCollection.Flags;
		public virtual IReadOnlyList<AssetCollection?> Dependencies => File.Dependencies;

		private readonly ProjectExporter m_exporter;
		private readonly Dictionary<AssetInfo, IExportCollection> m_assetCollections = new();

		private readonly IBuildSettings? m_buildSettings;
		private readonly ITagManager? m_tagManager;
		private readonly SceneExportCollection[] m_scenes;
	}
}
