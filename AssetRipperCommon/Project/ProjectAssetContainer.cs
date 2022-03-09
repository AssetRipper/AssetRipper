using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.AssetBundle;
using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.ResourceManager;
using AssetRipper.Core.Classes.TagManager;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Core.Project
{
	public class ProjectAssetContainer : IExportContainer
	{
		public ProjectAssetContainer(ProjectExporterBase exporter, CoreConfiguration options, VirtualSerializedFile file, IEnumerable<IUnityObjectBase> assets,
			IReadOnlyList<IExportCollection> collections)
		{
			m_exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
			if (options == null) throw new ArgumentNullException(nameof(options));
			m_IgnoreAssetBundleContentPaths = options.IgnoreAssetBundleContentPaths;
			VirtualFile = file ?? throw new ArgumentNullException(nameof(file));
			ExportLayout = file.Layout;

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
				else if (asset is IAssetBundle assetBundle)
				{
					AddBundleAssets(assetBundle);
				}
				else if (asset is IResourceManager resourceManager)
				{
					AddResources(resourceManager);
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

#warning TODO: get rid of IEnumerable. pass only main asset (issues: prefab, texture with sprites, animatorController)
		public bool TryGetAssetPathFromAssets(IEnumerable<IUnityObjectBase> assets, out IUnityObjectBase selectedAsset, out string assetPath)
		{
			selectedAsset = null;
			assetPath = string.Empty;
			if (m_pathAssets.Count > 0)
			{
				foreach (IUnityObjectBase asset in assets)
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

		public IUnityObjectBase GetAsset(long pathID)
		{
			return File.GetAsset(pathID);
		}

		public virtual IUnityObjectBase FindAsset(int fileIndex, long pathID)
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

		public virtual IUnityObjectBase GetAsset(int fileIndex, long pathID)
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

		public IUnityObjectBase FindAsset(ClassIDType classID)
		{
			return File.FindAsset(classID);
		}

		public virtual IUnityObjectBase FindAsset(ClassIDType classID, string name)
		{
			return File.FindAsset(classID, name);
		}

		public ClassIDType GetAssetType(long pathID)
		{
			return File.GetAssetType(pathID);
		}

		public long GetExportID(IUnityObjectBase asset)
		{
			if (m_assetCollections.TryGetValue(asset.AssetInfo, out IExportCollection collection))
			{
				return collection.GetExportID(asset);
			}

			return ExportIdHandler.GetMainExportID(asset);
		}

		public AssetType ToExportType(ClassIDType classID)
		{
			return m_exporter.ToExportType(classID);
		}

		public MetaPtr CreateExportPointer(IUnityObjectBase asset)
		{
			if (m_assetCollections.TryGetValue(asset.AssetInfo, out IExportCollection collection))
			{
				return collection.CreateExportPointer(asset, collection == CurrentCollection);
			}

			long exportID = ExportIdHandler.GetMainExportID(asset);
			return new MetaPtr(exportID, UnityGUID.MissingReference, AssetType.Meta);
		}

		public UnityGUID SceneNameToGUID(string name)
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
			return m_buildSettings == null ? $"level{sceneIndex}" : m_buildSettings.Scenes[sceneIndex].String;
		}

		public bool IsSceneDuplicate(int sceneIndex)
		{
			if (m_buildSettings == null)
			{
				return false;
			}

			string sceneName = m_buildSettings.Scenes[sceneIndex].String;
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
					return TagManagerConstants.UntaggedTag;
				case 1:
					return TagManagerConstants.RespawnTag;
				case 2:
					return TagManagerConstants.FinishTag;
				case 3:
					return TagManagerConstants.EditorOnlyTag;
				//case 4:
				case 5:
					return TagManagerConstants.MainCameraTag;
				case 6:
					return TagManagerConstants.PlayerTag;
				case 7:
					return TagManagerConstants.GameControllerTag;
			}
			if (m_tagManager != null)
			{
				// Unity doesn't verify tagID on export?
				int tagIndex = tagID - 20000;
				if (tagIndex < m_tagManager.Tags.Length)
				{
					if (tagIndex >= 0)
					{
						return m_tagManager.Tags[tagIndex].String;
					}
					else if (!m_tagManager.IsBrokenCustomTags())
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
				case TagManagerConstants.UntaggedTag:
					return 0;
				case TagManagerConstants.RespawnTag:
					return 1;
				case TagManagerConstants.FinishTag:
					return 2;
				case TagManagerConstants.EditorOnlyTag:
					return 3;
				case TagManagerConstants.MainCameraTag:
					return 5;
				case TagManagerConstants.PlayerTag:
					return 6;
				case TagManagerConstants.GameControllerTag:
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

		private void AddResources(IResourceManager manager)
		{
			foreach (NullableKeyValuePair<Utf8StringBase, PPtr<IUnityObjectBase>> kvp in manager.GetAssets())
			{
				IUnityObjectBase asset = kvp.Value.FindAsset(manager.SerializedFile);
				if (asset == null)
				{
					continue;
				}

				string resourcePath = kvp.Key.String;
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

		private void AddBundleAssets(IAssetBundle bundle)
		{
			string bundleName = bundle.GetAssetBundleName();
			string bundleDirectory = bundleName + ObjectUtils.DirectorySeparator;
			string directory = Path.Combine(AssetBundleFullPath, bundleName);
			foreach (NullableKeyValuePair<Utf8StringBase, IAssetInfo> kvp in bundle.GetAssets())
			{
				// skip shared bundle assets, because we need to export them in their bundle directory
				if (kvp.Value.AssetPtr.FileIndex != 0)
				{
					continue;
				}
				IUnityObjectBase asset = kvp.Value.AssetPtr.FindAsset(bundle.SerializedFile);
				if (asset == null)
				{
					continue;
				}

				string assetPath = kvp.Key.String;
				if (bundle.HasPathExtension())
				{
					// custom names may not have extensions
					int extensionIndex = assetPath.LastIndexOf('.');
					if (extensionIndex != -1)
					{
						assetPath = assetPath.Substring(0, extensionIndex);
					}
				}

				if (m_IgnoreAssetBundleContentPaths)
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
		public LayoutInfo Layout => File.Layout;
		public UnityVersion Version => File.Version;
		public Platform Platform => File.Platform;
		public TransferInstructionFlags Flags => File.Flags;
		public LayoutInfo ExportLayout { get; }
		public UnityVersion ExportVersion => ExportLayout.Version;
		public Platform ExportPlatform => ExportLayout.Platform;
		public virtual TransferInstructionFlags ExportFlags => ExportLayout.Flags | CurrentCollection.Flags;
		public virtual IReadOnlyList<FileIdentifier> Dependencies => File.Dependencies;

		private const string ResourcesKeyword = "Resources";
		private const string AssetBundleKeyword = "AssetBundles";
		private const string AssetsDirectory = UnityObjectBase.AssetsKeyword + ObjectUtils.DirectorySeparator;
		private const string ResourceFullPath = AssetsDirectory + ResourcesKeyword;
		//private const string AssetBundleFullPath = AssetsDirectory + AssetBundleKeyword;
		private const string AssetBundleFullPath = AssetsDirectory + "Asset_Bundles";

		private readonly ProjectExporterBase m_exporter;
		private readonly bool m_IgnoreAssetBundleContentPaths;
		private readonly Dictionary<Parser.Asset.AssetInfo, IExportCollection> m_assetCollections = new Dictionary<Parser.Asset.AssetInfo, IExportCollection>();
		private readonly Dictionary<IUnityObjectBase, ProjectAssetPath> m_pathAssets = new Dictionary<IUnityObjectBase, ProjectAssetPath>();

		private readonly IBuildSettings m_buildSettings;
		private readonly ITagManager m_tagManager;
		private readonly SceneExportCollection[] m_scenes;
	}
}
