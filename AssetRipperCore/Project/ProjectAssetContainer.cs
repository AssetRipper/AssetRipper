using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.AssetBundle;
using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.ResourceManager;
using AssetRipper.Core.Classes.TagManager;
using AssetRipper.Core.Classes.Utils;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Project.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using AssetInfo = AssetRipper.Core.Classes.AssetBundle.AssetInfo;
using Object = AssetRipper.Core.Classes.Object.Object;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Core.Project
{
	public class ProjectAssetContainer : IExportContainer
	{
		public ProjectAssetContainer(ProjectExporter exporter, CoreConfiguration options, VirtualSerializedFile file, IEnumerable<Object> assets,
			IReadOnlyList<IExportCollection> collections)
		{
			m_exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
			if(options == null) throw new ArgumentNullException(nameof(options));
			m_KeepAssetBundleContentPath = options.KeepAssetBundleContentPath;
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
			if (m_pathAssets.Count > 0)
			{
				foreach (Object asset in assets)
				{
					if (m_pathAssets.TryGetValue(asset, out ProjectAssetPath projectPath))
					{
						selectedAsset = asset;
						assetPath = projectPath.SubstituteExportPath(asset);
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

		public virtual Object FindAsset(int fileIndex, long pathID)
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

		public virtual Object GetAsset(int fileIndex, long pathID)
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

		public virtual Object FindAsset(ClassIDType classID, string name)
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
			return new MetaPtr(exportID, UnityGUID.MissingReference, AssetType.Meta);
		}

		public UnityGUID SceneNameToGUID(string name)
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
				if (m_pathAssets.TryGetValue(asset, out ProjectAssetPath projectPath))
				{
					// for paths like "Resources/inner/resources/extra/file" engine creates 2 resource entries
					// "inner/resources/extra/file" and "extra/file"
					if (projectPath.AssetPath.Length >= resourcePath.Length)
					{
						continue;
					}
				}
				m_pathAssets[asset] = new ProjectAssetPath(ResourceFullPath, resourcePath);
			}
		}

		private void AddBundleAssets(AssetBundle bundle)
		{
			string bundleName = AssetBundle.HasAssetBundleName(bundle.File.Version) ? bundle.AssetBundleName : bundle.File.Name;
			string bundleDirectory = bundleName + ObjectUtils.DirectorySeparator;
			string directory = Path.Combine(AssetBundleFullPath, bundleName);
			foreach (KeyValuePair<string, AssetInfo> kvp in bundle.Container)
			{
				// skip shared bundle assets, because we need to export them in their bundle directory
				if (kvp.Value.Asset.FileIndex != 0)
				{
					continue;
				}
				Object asset = kvp.Value.Asset.FindAsset(bundle.File);
				if (asset == null)
				{
					continue;
				}

				string assetPath = kvp.Key;
				if (AssetBundle.HasPathExtension(bundle.File.Version))
				{
					// custom names may not have extensions
					int extensionIndex = assetPath.LastIndexOf('.');
					if (extensionIndex != -1)
					{
						assetPath = assetPath.Substring(0, extensionIndex);
					}
				}

				if (m_KeepAssetBundleContentPath)
				{
					m_pathAssets.Add(asset, new ProjectAssetPath(string.Empty, assetPath));
				}
				else
				{
					if (assetPath.StartsWith(AssetsDirectory, StringComparison.OrdinalIgnoreCase))
					{
						assetPath = assetPath.Substring(AssetsDirectory.Length);
					}
					if (assetPath.StartsWith(bundleDirectory, StringComparison.OrdinalIgnoreCase))
					{
						assetPath = assetPath.Substring(bundleDirectory.Length);
					}
					m_pathAssets.Add(asset, new ProjectAssetPath(directory, assetPath));
				}
			}
#warning TODO: asset bundle may contain more assets than listed in Container. Need to export them in AssetBundleFullPath directory if KeepAssetBundleContentPath is false
		}

		public IExportCollection CurrentCollection { get; set; }
		public VirtualSerializedFile VirtualFile { get; }
		public virtual ISerializedFile File => CurrentCollection.File;
		public string Name => File.Name;
		public AssetLayout Layout => File.Layout;
		public UnityVersion Version => File.Version;
		public Platform Platform => File.Platform;
		public TransferInstructionFlags Flags => File.Flags;
		public AssetLayout ExportLayout { get; }
		public UnityVersion ExportVersion => ExportLayout.Info.Version;
		public Platform ExportPlatform => ExportLayout.Info.Platform;
		public virtual TransferInstructionFlags ExportFlags => ExportLayout.Info.Flags | CurrentCollection.Flags;
		public virtual IReadOnlyList<FileIdentifier> Dependencies => File.Dependencies;

		private const string ResourcesKeyword = "Resources";
		private const string AssetBundleKeyword = "AssetBundles";
		private const string AssetsDirectory = Object.AssetsKeyword + ObjectUtils.DirectorySeparator;
		private const string ResourceFullPath = AssetsDirectory + ResourcesKeyword;
		//private const string AssetBundleFullPath = AssetsDirectory + AssetBundleKeyword;
		private const string AssetBundleFullPath = AssetsDirectory + "Asset_Bundles";

		private readonly ProjectExporter m_exporter;
		private readonly bool m_KeepAssetBundleContentPath;
		private readonly Dictionary<Parser.Asset.AssetInfo, IExportCollection> m_assetCollections = new Dictionary<Parser.Asset.AssetInfo, IExportCollection>();
		private readonly Dictionary<Object, ProjectAssetPath> m_pathAssets = new Dictionary<Object, ProjectAssetPath>();

		private readonly BuildSettings m_buildSettings;
		private readonly TagManager m_tagManager;
		private readonly SceneExportCollection[] m_scenes;
	}
}
