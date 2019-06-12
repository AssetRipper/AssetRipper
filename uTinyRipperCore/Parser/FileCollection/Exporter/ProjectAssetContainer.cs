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
		public ProjectAssetContainer(ProjectExporter exporter, VirtualSerializedFile file, IEnumerable<Object> assets,
			IReadOnlyList<IExportCollection> collections, ExportOptions options)
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
						m_resourceManager = (ResourceManager)asset;
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

#warning TODO: get rid of IEnumerable. pass only main asset (issues: prefab, texture with sprites, animatorController)
		public bool TryGetResourcePathFromAssets(IEnumerable<Object> assets, out Object selectedAsset, out string resourcePath)
		{
			selectedAsset = null;
			resourcePath = string.Empty;
			if (m_resourceManager == null)
			{
				return false;
			}

			foreach (Object asset in assets)
			{
				if (m_resourceManager.TryGetResourcePathFromAsset(asset, out resourcePath))
				{
					selectedAsset = asset;
					return true;
				}
			}

			return false;
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

			// Unity doesn't verify tagID on export?
			int tagIndex = tagID - 20000;
			if (tagIndex >= m_tagManager.Tags.Count)
			{
				return $"unknown_{tagID}";
			}
			return m_tagManager.Tags[tagIndex];
		}

		public IExportCollection CurrentCollection { get; set; }
		public VirtualSerializedFile VirtualFile { get; }
		public ISerializedFile File => CurrentCollection.File;
		public Version Version => File.Version;
		public Platform Platform => File.Platform;
		public TransferInstructionFlags Flags => File.Flags;
		public Version ExportVersion { get; }
		public Platform ExportPlatform { get; }
		public TransferInstructionFlags ExportFlags => m_exportFlags | CurrentCollection.Flags;

		private readonly ProjectExporter m_exporter;
		private readonly Dictionary<Object, IExportCollection> m_assetCollections = new Dictionary<Object, IExportCollection>();

		private readonly BuildSettings m_buildSettings;
		private readonly TagManager m_tagManager;
		private readonly ResourceManager m_resourceManager;
		private readonly SceneExportCollection[] m_scenes;
		private readonly TransferInstructionFlags m_exportFlags;
	}
}
