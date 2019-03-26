using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters.Classes;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.AssetExporters
{
	public class ProjectAssetContainer : IExportContainer
	{
		public ProjectAssetContainer(ProjectExporter exporter, IEnumerable<Object> assets, VirtualSerializedFile file, IReadOnlyList<IExportCollection> collections)
		{
			if(exporter == null)
			{
				throw new ArgumentNullException(nameof(exporter));
			}
			if (file == null)
			{
				throw new ArgumentNullException(nameof(file));
			}
			if (collections == null)
			{
				throw new ArgumentNullException(nameof(collections));
			}
			m_exporter = exporter;
			VirtualFile = file;
			m_collections = collections;

			foreach (Object asset in assets)
			{
				switch (asset.ClassID)
				{
					case ClassIDType.BuildSettings:
						m_buildSettings = (BuildSettings)asset;
						break;

					case ClassIDType.TagManager:
						m_tagManager = (TagManager)asset;
						break;
				}
			}

			List<SceneExportCollection> scenes = new List<SceneExportCollection>();
			foreach (IExportCollection collection in collections)
			{
				foreach (Object asset in collection.Assets)
				{
#warning TODO: unique asset:collection (m_assetCollections.Add)
					m_assetCollections[asset] = collection;
				}
				if(collection is SceneExportCollection scene)
				{
					scenes.Add(scene);
				}
			}
			m_scenes = scenes.ToArray();
		}

		public Object FindAsset(int fileIndex, long pathID)
		{
			if(fileIndex == VirtualSerializedFile.VirtualFileIndex)
			{
				return VirtualFile.FindAsset(pathID);
			}
			else
			{
				return File.FindAsset(fileIndex, pathID);
			}
		}

		public Object FindAsset(ClassIDType classID)
		{
			Object asset = VirtualFile.FindAsset(classID);
			return asset ?? File.FindAsset(classID);
		}

		public Object FindAsset(ClassIDType classID, string name)
		{
			Object asset = VirtualFile.FindAsset(classID, name);
			return asset ?? File.FindAsset(classID, name);
		}

		public long GetExportID(Object asset)
		{
			if (m_assetCollections.TryGetValue(asset, out IExportCollection collection))
			{
				return collection.GetExportID(asset);
			}

			if (Config.IsExportDependencies)
			{
				throw new InvalidOperationException($"Object {asset} wasn't found in any export collection");
			}
			else
			{
				return ExportCollection.GetMainExportID(asset);
			}
		}

		public AssetType ToExportType(ClassIDType classID)
		{
			return m_exporter.ToExportType(classID);
		}

		public ExportPointer CreateExportPointer(Object asset)
		{
			if (m_assetCollections.TryGetValue(asset, out IExportCollection collection))
			{
				return collection.CreateExportPointer(asset, collection == CurrentCollection);
			}

			if (Config.IsExportDependencies)
			{
				//throw new InvalidOperationException($"Object {asset} wasn't found in any export collection");
			}
			long exportID = ExportCollection.GetMainExportID(asset);
			return new ExportPointer(exportID, EngineGUID.MissingReference, AssetType.Meta);
		}

		public EngineGUID SceneNameToGUID(string name)
		{
			if(m_buildSettings == null)
			{
				return default;
			}

			int index = m_buildSettings.Scenes.IndexOf(name);
			if(index == -1)
			{
				throw new Exception($"Scene '{name}' hasn't been found in build settings");
			}

			string fileName = SceneExportCollection.SceneIndexToFileName(index, Version);
			foreach (SceneExportCollection scene in m_scenes)
			{
				if(scene.Name == fileName)
				{
					return scene.GUID;
				}
			}
			return default;
		}

		public string SceneIndexToName(int sceneIndex)
		{
			return m_buildSettings == null ? $"level{sceneIndex}" : m_buildSettings.Scenes[sceneIndex];
		}

		public bool IsSceneDuplicate(int sceneIndex)
		{
			if (m_buildSettings == null)
			{
				return false;
			}

			string sceneName = m_buildSettings.Scenes[sceneIndex];
			for (int i = 0; i < m_buildSettings.Scenes.Count; i++)
			{
				if (m_buildSettings.Scenes[i] == sceneName)
				{
					if (i != sceneIndex)
					{
						return true;
					}
				}
			}
			return false;
		}

		public string TagIDToName(int tagID)
		{
			const string UntaggedTag = "Untagged";
			switch (tagID)
			{
				case 0:
					return UntaggedTag;
				case 1:
					return "Respawn";
				case 2:
					return "Finish";
				case 3:
					return "EditorOnly";
				//case 4:
				case 5:
					return "MainCamera";
				case 6:
					return "Player";
				case 7:
					return "GameController";
			}
			if(m_tagManager == null)
			{
				return UntaggedTag;
			}
			if(m_tagManager.Tags.Count == 0)
			{
				return $"unknown_{tagID}";
			}
			return m_tagManager.Tags[tagID - 20000];
		}

		public IExportCollection CurrentCollection { get; set; }
		public VirtualSerializedFile VirtualFile { get; }
		public ISerializedFile File => CurrentCollection.File;
		public Version Version => File.Version;
		public Platform Platform => File.Platform;
		public TransferInstructionFlags Flags => File.Flags;
		public Version ExportVersion { get; } = new Version(2017, 3, 0, VersionType.Final, 3);
		public Platform ExportPlatform => Platform.NoTarget;
		public TransferInstructionFlags ExportFlags => TransferInstructionFlags.NoTransferInstructionFlags | CurrentCollection.Flags;

		private readonly ProjectExporter m_exporter;
		private readonly IReadOnlyList<IExportCollection> m_collections;
		private readonly Dictionary<Object, IExportCollection> m_assetCollections = new Dictionary<Object, IExportCollection>();

		private BuildSettings m_buildSettings;
		private TagManager m_tagManager;
		private SceneExportCollection[] m_scenes;
	}
}
