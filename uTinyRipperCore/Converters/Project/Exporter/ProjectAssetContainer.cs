using System;
using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.Project.Classes;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Converters
{
	public class ProjectAssetContainer : IExportContainer
	{
		public ProjectAssetContainer(ProjectExporter exporter, VirtualSerializedFile file, IEnumerable<Object> assets,
			IReadOnlyList<IExportCollection> collections, ExportOptions options)
		{
			if (exporter == null)
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

			ExportVersion = options.Version;
			ExportPlatform = options.Platform;
			m_exportFlags = options.Flags;

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

					case ClassIDType.ResourceManager:
						AddResources((ResourceManager)asset);
						break;
				}
			}

			List<SceneExportCollection> scenes = new List<SceneExportCollection>();
			foreach (IExportCollection collection in collections)
			{
				foreach (Object asset in collection.Assets)
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

#warning TODO: get rid of IEnumerable. pass only main asset (issues: prefab, texture with sprites, animatorController)
		public bool TryGetResourcePathFromAssets(IEnumerable<Object> assets, out Object selectedAsset, out string resourcePath)
		{
			selectedAsset = null;
			resourcePath = string.Empty;
			if (m_resources.Count > 0)
			{
				foreach (Object asset in assets)
				{
					if (m_resources.TryGetValue(asset, out string path))
					{
						selectedAsset = asset;
						resourcePath = ResourceManager.ResourceToExportPath(asset, path);
						return true;
					}
				}
			}

			return false;
		}

		public Object GetAsset(long pathID)
		{
			return File.GetAsset(pathID);
		}

		public Object FindAsset(int fileIndex, long pathID)
		{
			if (fileIndex == VirtualSerializedFile.VirtualFileIndex)
			{
				return VirtualFile.FindAsset(pathID);
			}
			else
			{
				return File.FindAsset(fileIndex, pathID);
			}
		}

		public Object GetAsset(int fileIndex, long pathID)
		{
			if (fileIndex == VirtualSerializedFile.VirtualFileIndex)
			{
				return VirtualFile.GetAsset(pathID);
			}
			else
			{
				return File.GetAsset(fileIndex, pathID);
			}
		}

		public Object FindAsset(ClassIDType classID)
		{
			return File.FindAsset(classID);
		}

		public Object FindAsset(ClassIDType classID, string name)
		{
			return File.FindAsset(classID, name);
		}

		public ClassIDType GetAssetType(long pathID)
		{
			return File.GetAssetType(pathID);
		}

		public long GetExportID(Object asset)
		{
			if (m_assetCollections.TryGetValue(asset.AssetInfo, out IExportCollection collection))
			{
				return collection.GetExportID(asset);
			}

			return ExportCollection.GetMainExportID(asset);
		}

		public AssetType ToExportType(ClassIDType classID)
		{
			return m_exporter.ToExportType(classID);
		}

		public ExportPointer CreateExportPointer(Object asset)
		{
			if (m_assetCollections.TryGetValue(asset.AssetInfo, out IExportCollection collection))
			{
				return collection.CreateExportPointer(asset, collection == CurrentCollection);
			}

			long exportID = ExportCollection.GetMainExportID(asset);
			return new ExportPointer(exportID, GUID.MissingReference, AssetType.Meta);
		}

		public GUID SceneNameToGUID(string name)
		{
			if (m_buildSettings == null)
			{
				return default;
			}

			int index = m_buildSettings.Scenes.IndexOf(name);
			if (index == -1)
			{
				throw new Exception($"Scene '{name}' hasn't been found in build settings");
			}

			string fileName = SceneExportCollection.SceneIndexToFileName(index, Version);
			foreach (SceneExportCollection scene in m_scenes)
			{
				if (scene.Name == fileName)
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
			if (m_tagManager == null)
			{
				return UntaggedTag;
			}

			// Unity doesn't verify tagID on export?
			int tagIndex = tagID - 20000;
			if (tagIndex >= m_tagManager.Tags.Count)
			{
				return $"unknown_{tagID}";
			}
			return m_tagManager.Tags[tagIndex];
		}

		private void AddResources(ResourceManager manager)
		{
			foreach (KeyValuePair<string, PPtr<Object>> kvp in manager.Container)
			{
				Object asset = kvp.Value.FindAsset(manager.File);
				if (asset == null)
				{
					continue;
				}
				m_resources.Add(asset, kvp.Key);
			}
		}

		public IExportCollection CurrentCollection { get; set; }
		public VirtualSerializedFile VirtualFile { get; }
		public ISerializedFile File => CurrentCollection.File;
		public string Name => File.Name;
		public Version Version => File.Version;
		public Platform Platform => File.Platform;
		public TransferInstructionFlags Flags => File.Flags;
		public Version ExportVersion { get; }
		public Platform ExportPlatform { get; }
		public TransferInstructionFlags ExportFlags => m_exportFlags | CurrentCollection.Flags;
		public IReadOnlyList<FileIdentifier> Dependencies => File.Dependencies;

		private readonly ProjectExporter m_exporter;
		private readonly Dictionary<AssetInfo, IExportCollection> m_assetCollections = new Dictionary<AssetInfo, IExportCollection>();
		private readonly Dictionary<Object, string> m_resources = new Dictionary<Object, string>();

		private readonly BuildSettings m_buildSettings;
		private readonly TagManager m_tagManager;
		private readonly SceneExportCollection[] m_scenes;
		private readonly TransferInstructionFlags m_exportFlags;
	}
}
