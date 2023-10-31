using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Primitives;
using AssetRipper.Processing.Scenes;
using AssetRipper.SourceGenerated.Classes.ClassID_141;
using AssetRipper.SourceGenerated.Extensions;


namespace AssetRipper.Export.UnityProjects
{
	public class ProjectAssetContainer : IExportContainer
	{
		public ProjectAssetContainer(ProjectExporter exporter, TemporaryAssetCollection file, IEnumerable<IUnityObjectBase> assets,
			IReadOnlyList<IExportCollection> collections)
		{
			m_exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
			VirtualFile = file ?? throw new ArgumentNullException(nameof(file));
			CurrentCollection = null!;

			m_buildSettings = assets.OfType<IBuildSettings>().FirstOrDefault();

			List<SceneExportCollection> scenes = new();
			foreach (IExportCollection collection in collections)
			{
				foreach (IUnityObjectBase asset in collection.Assets)
				{
#warning TODO: unique asset:collection (m_assetCollections.Add)
					m_assetCollections[asset] = collection;
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
			if (m_assetCollections.TryGetValue(asset, out IExportCollection? collection))
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
			if (m_assetCollections.TryGetValue(asset, out IExportCollection? collection))
			{
				return collection.CreateExportPointer(asset, collection == CurrentCollection);
			}

			return MetaPtr.CreateMissingReference(asset.ClassID, AssetType.Meta);
		}

		public UnityGuid SceneNameToGUID(string name)
		{
			if (m_buildSettings == null)
			{
				return default;
			}

			int index = m_buildSettings.Scenes.IndexOf(s => s.String == name);
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

		public bool IsSceneDuplicate(int sceneIndex) => SceneHelpers.IsSceneDuplicate(sceneIndex, m_buildSettings);

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
		private readonly Dictionary<IUnityObjectBase, IExportCollection> m_assetCollections = new();

		private readonly IBuildSettings? m_buildSettings;
		private readonly SceneExportCollection[] m_scenes;
	}
}
