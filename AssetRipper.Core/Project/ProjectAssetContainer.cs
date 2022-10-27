using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Core.Utils;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated.Classes.ClassID_141;
using AssetRipper.SourceGenerated.Classes.ClassID_142;
using AssetRipper.SourceGenerated.Classes.ClassID_147;
using AssetRipper.SourceGenerated.Classes.ClassID_78;
using AssetRipper.SourceGenerated.Subclasses.AssetInfo;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Object_;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;
using System.Collections.Generic;
using System.IO;


namespace AssetRipper.Core.Project
{
	public class ProjectAssetContainer : IExportContainer, IProjectAssetContainer
	{
		public ProjectAssetContainer(ProjectExporter exporter, CoreConfiguration options, TemporaryAssetCollection file, IEnumerable<IUnityObjectBase> assets,
			IReadOnlyList<IExportCollection> collections)
		{
			m_exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
			VirtualFile = file ?? throw new ArgumentNullException(nameof(file));
			ExportLayout = new LayoutInfo(file.Version, file.Platform, file.Flags);

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
					AddBundleAssets(assetBundle, options.BundledAssetsExportMode);
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

			string fileName = SceneExportHelpers.SceneIndexToFileName(index, Version);
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

		public bool IsSceneDuplicate(int sceneIndex) => SceneExportHelpers.IsSceneDuplicate(sceneIndex, m_buildSettings);

		public string TagIDToName(int tagID)
		{
			return m_tagManager.TagIDToName(tagID);
		}

		public ushort TagNameToID(string tagName)
		{
			return m_tagManager.TagNameToID(tagName);
		}

		private static void AddResources(IResourceManager manager)
		{
			foreach (AccessPairBase<Utf8String, IPPtr_Object_> kvp in manager.Container_C147)
			{
				IUnityObjectBase? asset = kvp.Value.TryGetAsset(manager.Collection);
				if (asset is null)
				{
					continue;
				}

				string resourcePath = Path.Combine(ResourceFullPath, kvp.Key.String);
				if (asset.OriginalPath is null)
				{
					asset.OriginalPath = resourcePath;
				}
				else if (asset.OriginalPath.Length < resourcePath.Length)
				{
					// for paths like "Resources/inner/resources/extra/file" engine creates 2 resource entries
					// "inner/resources/extra/file" and "extra/file"
					asset.OriginalPath = resourcePath;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// TODO: Asset bundles usually contain more assets than listed in <see cref="IAssetBundle.Container_C142"/>. 
		/// Need to export them in AssetBundleFullPath directory if <see cref="m_BundledAssetsExportMode"/> is <see cref="BundledAssetsExportMode.GroupByBundleName"/>.
		/// Or maybe remove that mode entirely. It has dubious utility.
		/// </remarks>
		/// <param name="bundle"></param>
		/// <exception cref="Exception"></exception>
		private static void AddBundleAssets(IAssetBundle bundle, BundledAssetsExportMode bundledAssetsExportMode)
		{
			string bundleName = bundle.GetAssetBundleName();
			string bundleDirectory = bundleName + ObjectUtils.DirectorySeparator;
			string directory = Path.Combine(AssetBundleFullPath, bundleName);
			foreach (AccessPairBase<Utf8String, IAssetInfo> kvp in bundle.Container_C142)
			{
				// skip shared bundle assets, because we need to export them in their bundle directory
				if (kvp.Value.Asset.FileID != 0)
				{
					continue;
				}

				UnityObjectBase? asset = kvp.Value.Asset.TryGetAsset(bundle.Collection);
				if (asset is null)
				{
					continue;
				}

				asset.AssetBundleName = bundleName;

				string assetPath = EnsurePathNotRooted(kvp.Key.String);
				if (string.IsNullOrEmpty(assetPath))
				{
					continue;
				}

				switch (bundledAssetsExportMode)
				{
					case BundledAssetsExportMode.DirectExport:
						if (assetPath.StartsWith(AssetsDirectory, StringComparison.Ordinal))
						{
							asset.OriginalPath = assetPath;
						}
						else if (assetPath.StartsWith(AssetsDirectory, StringComparison.OrdinalIgnoreCase))
						{
							asset.OriginalPath = $"{AssetsDirectory}{assetPath.AsSpan(AssetsDirectory.Length)}";
						}
						else
						{
							asset.OriginalPath = AssetsDirectory + assetPath;
						}
						break;
					case BundledAssetsExportMode.GroupByBundleName:
						if (assetPath.StartsWith(AssetsDirectory, StringComparison.OrdinalIgnoreCase))
						{
							assetPath = assetPath.Substring(AssetsDirectory.Length);
						}
						if (assetPath.StartsWith(bundleDirectory, StringComparison.OrdinalIgnoreCase))
						{
							assetPath = assetPath.Substring(bundleDirectory.Length);
						}
						asset.OriginalPath = Path.Combine(directory, assetPath);
						break;
					case BundledAssetsExportMode.GroupByAssetType:
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(bundledAssetsExportMode), $"Invalid {nameof(BundledAssetsExportMode)} : {bundledAssetsExportMode}");
				}
			}
		}

		private static string EnsurePathNotRooted(string assetPath)
		{
			if (Path.IsPathRooted(assetPath))
			{
				string[] splitPath = assetPath.Split('/');
				for (int i = 0; i < splitPath.Length; i++)
				{
					string pathSection = splitPath[i];
					if (string.Equals(pathSection, ExportCollection.AssetsKeyword, StringComparison.OrdinalIgnoreCase))
					{
						return string.Join(ObjectUtils.DirectorySeparator, new ArraySegment<string>(splitPath, i, splitPath.Length - i));
					}
				}
				return string.Empty;
			}
			else
			{
				return assetPath;
			}
		}

		public IExportCollection CurrentCollection { get; set; }
		public TemporaryAssetCollection VirtualFile { get; }
		public virtual AssetCollection File => CurrentCollection.File;
		public string Name => File.Name;
		public UnityVersion Version => File.Version;
		public BuildTarget Platform => File.Platform;
		public TransferInstructionFlags Flags => File.Flags;
		public LayoutInfo ExportLayout { get; }
		public UnityVersion ExportVersion => ExportLayout.Version;
		public BuildTarget ExportPlatform => ExportLayout.Platform;
		public virtual TransferInstructionFlags ExportFlags => ExportLayout.Flags | CurrentCollection.Flags;
		public virtual IReadOnlyList<AssetCollection?> Dependencies => File.Dependencies;

		private const string ResourcesKeyword = "Resources";
		private const string AssetBundleKeyword = "AssetBundles";
		private const string AssetsDirectory = ExportCollection.AssetsKeyword + ObjectUtils.DirectorySeparator;
		private const string ResourceFullPath = AssetsDirectory + ResourcesKeyword;
		//private const string AssetBundleFullPath = AssetsDirectory + AssetBundleKeyword;
		private const string AssetBundleFullPath = AssetsDirectory + "Asset_Bundles";

		private readonly ProjectExporter m_exporter;
		private readonly Dictionary<AssetInfo, IExportCollection> m_assetCollections = new();

		private readonly IBuildSettings? m_buildSettings;
		private readonly ITagManager? m_tagManager;
		private readonly SceneExportCollection[] m_scenes;
	}
}
