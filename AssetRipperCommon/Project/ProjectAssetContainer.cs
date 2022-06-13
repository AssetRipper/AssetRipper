using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Misc;
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
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Core.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_141;
using AssetRipper.SourceGenerated.Classes.ClassID_142;
using AssetRipper.SourceGenerated.Classes.ClassID_147;
using AssetRipper.SourceGenerated.Classes.ClassID_78;
using AssetRipper.SourceGenerated.Subclasses.AssetInfo;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Object_;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;


namespace AssetRipper.Core.Project
{
	public class ProjectAssetContainer : IExportContainer, IProjectAssetContainer
	{
		public ProjectAssetContainer(ProjectExporter exporter, CoreConfiguration options, VirtualSerializedFile file, IEnumerable<IUnityObjectBase> assets,
			IReadOnlyList<IExportCollection> collections)
		{
			m_exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
			m_BundledAssetsExportMode = options.BundledAssetsExportMode;
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
		public bool TryGetAssetPathFromAssets(IEnumerable<IUnityObjectBase> assets, [NotNullWhen(true)] out IUnityObjectBase? selectedAsset, out string assetPath)
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

		public virtual IUnityObjectBase? FindAsset(int fileIndex, long pathID)
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

		public IUnityObjectBase? FindAsset(ClassIDType classID)
		{
			return File.FindAsset(classID);
		}

		public virtual IUnityObjectBase? FindAsset(ClassIDType classID, string name)
		{
			return File.FindAsset(classID, name);
		}

		public ClassIDType GetAssetType(long pathID)
		{
			return File.GetAssetType(pathID);
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

			long exportID = ExportIdHandler.GetMainExportID(asset);
			return new MetaPtr(exportID, UnityGUID.MissingReference, AssetType.Meta);
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
			return m_buildSettings == null ? $"level{sceneIndex}" : m_buildSettings.Scenes_C141[sceneIndex].String;
		}

		public bool IsSceneDuplicate(int sceneIndex)
		{
			if (m_buildSettings == null)
			{
				return false;
			}

			string sceneName = m_buildSettings.Scenes_C141[sceneIndex].String;
			for (int i = 0; i < m_buildSettings.Scenes_C141.Count; i++)
			{
				if (m_buildSettings.Scenes_C141[i] == sceneName)
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
					return Classes.TagManager.TagManagerConstants.UntaggedTag;
				case 1:
					return Classes.TagManager.TagManagerConstants.RespawnTag;
				case 2:
					return Classes.TagManager.TagManagerConstants.FinishTag;
				case 3:
					return Classes.TagManager.TagManagerConstants.EditorOnlyTag;
				//case 4:
				case 5:
					return Classes.TagManager.TagManagerConstants.MainCameraTag;
				case 6:
					return Classes.TagManager.TagManagerConstants.PlayerTag;
				case 7:
					return Classes.TagManager.TagManagerConstants.GameControllerTag;
			}
			if (m_tagManager != null)
			{
				// Unity doesn't verify tagID on export?
				int tagIndex = tagID - 20000;
				if (tagIndex < m_tagManager.Tags_C78.Count)
				{
					if (tagIndex >= 0)
					{
						return m_tagManager.Tags_C78[tagIndex].String;
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
				case Classes.TagManager.TagManagerConstants.UntaggedTag:
					return 0;
				case Classes.TagManager.TagManagerConstants.RespawnTag:
					return 1;
				case Classes.TagManager.TagManagerConstants.FinishTag:
					return 2;
				case Classes.TagManager.TagManagerConstants.EditorOnlyTag:
					return 3;
				case Classes.TagManager.TagManagerConstants.MainCameraTag:
					return 5;
				case Classes.TagManager.TagManagerConstants.PlayerTag:
					return 6;
				case Classes.TagManager.TagManagerConstants.GameControllerTag:
					return 7;
			}
			if (m_tagManager != null)
			{
				for (int i = 0; i < m_tagManager.Tags_C78.Count; i++)
				{
					if (m_tagManager.Tags_C78[i] == tagName)
					{
						return (ushort)(20000 + i);
					}
				}
			}
			return 0;
		}

		private void AddResources(IResourceManager manager)
		{
			foreach (NullableKeyValuePair<Utf8String, IPPtr_Object_> kvp in manager.Container_C147)
			{
				IUnityObjectBase? asset = kvp.Value.FindAsset(manager.SerializedFile);
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
			if (m_BundledAssetsExportMode == BundledAssetsExportMode.GroupByAssetType)
			{
				return;
			}

			string bundleName = bundle.GetAssetBundleName();
			string bundleDirectory = bundleName + ObjectUtils.DirectorySeparator;
			string directory = Path.Combine(AssetBundleFullPath, bundleName);
			foreach (NullableKeyValuePair<Utf8String, IAssetInfo> kvp in bundle.Container_C142)
			{
				// skip shared bundle assets, because we need to export them in their bundle directory
				if (kvp.Value.Asset.FileIndex != 0)
				{
					continue;
				}
				UnityObjectBase? asset = kvp.Value.Asset.FindAsset(bundle.SerializedFile);
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

				if (m_BundledAssetsExportMode == BundledAssetsExportMode.DirectExport)
				{
					m_pathAssets.Add(asset, new ProjectAssetPath(string.Empty, assetPath));
				}
				else if (m_BundledAssetsExportMode == BundledAssetsExportMode.GroupByBundleName)
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
				else
				{
					throw new Exception($"Invalid {nameof(BundledAssetsExportMode)} for {nameof(m_BundledAssetsExportMode)} : {m_BundledAssetsExportMode}");
				}
			}
#warning TODO: asset bundle may contain more assets than listed in Container. Need to export them in AssetBundleFullPath directory if m_BundledAssetsExportMode is GroupByBundleName
		}

		public IExportCollection CurrentCollection { get; set; }
		public VirtualSerializedFile VirtualFile { get; }
		public virtual ISerializedFile File => CurrentCollection.File;
		public string Name => File.Name;
		public LayoutInfo Layout => File.Layout;
		public UnityVersion Version => File.Version;
		public BuildTarget Platform => File.Platform;
		public TransferInstructionFlags Flags => File.Flags;
		public LayoutInfo ExportLayout { get; }
		public UnityVersion ExportVersion => ExportLayout.Version;
		public BuildTarget ExportPlatform => ExportLayout.Platform;
		public virtual TransferInstructionFlags ExportFlags => ExportLayout.Flags | CurrentCollection.Flags;
		public virtual IReadOnlyList<FileIdentifier> Dependencies => File.Dependencies;

		private const string ResourcesKeyword = "Resources";
		private const string AssetBundleKeyword = "AssetBundles";
		private const string AssetsDirectory = UnityObjectBase.AssetsKeyword + ObjectUtils.DirectorySeparator;
		private const string ResourceFullPath = AssetsDirectory + ResourcesKeyword;
		//private const string AssetBundleFullPath = AssetsDirectory + AssetBundleKeyword;
		private const string AssetBundleFullPath = AssetsDirectory + "Asset_Bundles";

		private readonly ProjectExporter m_exporter;
		private readonly BundledAssetsExportMode m_BundledAssetsExportMode;
		private readonly Dictionary<Parser.Asset.AssetInfo, IExportCollection> m_assetCollections = new Dictionary<Parser.Asset.AssetInfo, IExportCollection>();
		private readonly Dictionary<IUnityObjectBase, ProjectAssetPath> m_pathAssets = new Dictionary<IUnityObjectBase, ProjectAssetPath>();

		private readonly IBuildSettings? m_buildSettings;
		private readonly ITagManager? m_tagManager;
		private readonly SceneExportCollection[] m_scenes;
	}
}
