using System;
using System.Collections.Generic;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Layout;
using uTinyRipper.Project;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Converters
{
	public class ProjectAssetContainer : IExportContainer
	{
		public ProjectAssetContainer(ProjectExporter exporter, VirtualSerializedFile file, IEnumerable<Object> assets,
			IReadOnlyList<IExportCollection> collections)
		{
			m_exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
			VirtualFile = file ?? throw new ArgumentNullException(nameof(file));
			ExportLayout = file.Layout;

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

					case ClassIDType.AssetBundle:
						AddBundleAssets((AssetBundle)asset);
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
		public bool TryGetAssetPathFromAssets(IEnumerable<Object> assets, out Object selectedAsset, out string assetPath)
		{
			selectedAsset = null;
			assetPath = string.Empty;
			if (m_resources.Count > 0 || m_bundleAssets.Count > 0)
			{
				foreach (Object asset in assets)
				{
					if (m_resources.TryGetValue(asset, out string resourcePath))
					{
						selectedAsset = asset;
						assetPath = PathUtils.SubstituteResourcePath(asset, resourcePath);
						return true;
					}
					if (m_bundleAssets.TryGetValue(asset, out string bundleAssetPath))
					{
						selectedAsset = asset;
						assetPath = PathUtils.SubstituteAssetBundlePath(asset, bundleAssetPath);
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

		public MetaPtr CreateExportPointer(Object asset)
		{
			if (m_assetCollections.TryGetValue(asset.AssetInfo, out IExportCollection collection))
			{
				return collection.CreateExportPointer(asset, collection == CurrentCollection);
			}

			long exportID = ExportCollection.GetMainExportID(asset);
			return new MetaPtr(exportID, GUID.MissingReference, AssetType.Meta);
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
			for (int i = 0; i < m_buildSettings.Scenes.Length; i++)
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
			switch (tagID)
			{
				case 0:
					return TagManager.UntaggedTag;
				case 1:
					return TagManager.RespawnTag;
				case 2:
					return TagManager.FinishTag;
				case 3:
					return TagManager.EditorOnlyTag;
				//case 4:
				case 5:
					return TagManager.MainCameraTag;
				case 6:
					return TagManager.PlayerTag;
				case 7:
					return TagManager.GameControllerTag;
			}
			if (m_tagManager != null)
			{
				// Unity doesn't verify tagID on export?
				int tagIndex = tagID - 20000;
				if (tagIndex < m_tagManager.Tags.Length)
				{
					if (tagIndex >= 0)
					{
						return m_tagManager.Tags[tagIndex];
					}
					else if (!TagManager.IsBrokenCustomTags(m_tagManager.File.Version))
					{
						throw new Exception($"Unknown default tag {tagID}");
					}
				}
			}
			return $"unknown_{tagID}";
		}

		public ushort TagNameToID(string tagName)
		{
			switch (tagName)
			{
				case TagManager.UntaggedTag:
					return 0;
				case TagManager.RespawnTag:
					return 1;
				case TagManager.FinishTag:
					return 2;
				case TagManager.EditorOnlyTag:
					return 3;
				case TagManager.MainCameraTag:
					return 5;
				case TagManager.PlayerTag:
					return 6;
				case TagManager.GameControllerTag:
					return 7;
			}
			if (m_tagManager != null)
			{
				for (int i = 0; i < m_tagManager.Tags.Length; i++)
				{
					if (m_tagManager.Tags[i] == tagName)
					{
						return (ushort)(20000 + i);
					}
				}
			}
			return 0;
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

				string resourcePath = kvp.Key;
				if (m_resources.ContainsKey(asset))
				{
					// for paths like "Resources/inner/resources/extra/file" engine creates 2 resource entries
					// "inner/resources/extra/file" and "extra/file"
					if (m_resources[asset].Length < resourcePath.Length)
					{
						m_resources[asset] = resourcePath;
					}
				}
				else
				{
					m_resources.Add(asset, resourcePath);
				}
			}
		}

		private void AddBundleAssets(AssetBundle bundle)
		{
			foreach (KeyValuePair<string, Classes.AssetBundles.AssetInfo> kvp in bundle.Container)
			{
				Object asset = kvp.Value.Asset.FindAsset(bundle.File);
				if (asset != null)
				{
					string assetPath = kvp.Key;
					if (AssetBundle.HasPathExtension(bundle.File.Version))
					{
						assetPath = assetPath.Substring(0, assetPath.LastIndexOf('.'));
					}
					m_bundleAssets.Add(asset, assetPath);
				}
			}
		}

		public IExportCollection CurrentCollection { get; set; }
		public VirtualSerializedFile VirtualFile { get; }
		public ISerializedFile File => CurrentCollection.File;
		public string Name => File.Name;
		public AssetLayout Layout => File.Layout;
		public Version Version => File.Version;
		public Platform Platform => File.Platform;
		public TransferInstructionFlags Flags => File.Flags;
		public AssetLayout ExportLayout { get; }
		public Version ExportVersion => ExportLayout.Info.Version;
		public Platform ExportPlatform => ExportLayout.Info.Platform;
		public TransferInstructionFlags ExportFlags => ExportLayout.Info.Flags | CurrentCollection.Flags;
		public IReadOnlyList<FileIdentifier> Dependencies => File.Dependencies;

		private readonly ProjectExporter m_exporter;
		private readonly Dictionary<AssetInfo, IExportCollection> m_assetCollections = new Dictionary<AssetInfo, IExportCollection>();
		// Both ResourceManager and AssetBundle should neither exist in the same ProjectAssetContainer nor share asset Objects,
		// but just in case they somehow do, keeping m_resources and m_assetBundlePaths separately rather than merging the two.
		private readonly Dictionary<Object, string> m_resources = new Dictionary<Object, string>();
		// Also assume that there's at most a single AssetBundle in the ProjectAssetContainer, so we don't need to disambiguate
		// between multiple asset bundle names.
		private readonly Dictionary<Object, string> m_bundleAssets = new Dictionary<Object, string>();

		private readonly BuildSettings m_buildSettings;
		private readonly TagManager m_tagManager;
		private readonly SceneExportCollection[] m_scenes;
	}
}
