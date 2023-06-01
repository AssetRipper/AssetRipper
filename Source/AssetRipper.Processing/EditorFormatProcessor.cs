using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Utils;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Processing.AnimationClips;
using AssetRipper.Processing.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_129;
using AssetRipper.SourceGenerated.Classes.ClassID_142;
using AssetRipper.SourceGenerated.Classes.ClassID_147;
using AssetRipper.SourceGenerated.Classes.ClassID_157;
using AssetRipper.SourceGenerated.Classes.ClassID_19;
using AssetRipper.SourceGenerated.Classes.ClassID_196;
using AssetRipper.SourceGenerated.Classes.ClassID_218;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_30;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_47;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_78;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.AssetInfo;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Object;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;

namespace AssetRipper.Processing
{
	/// <summary>
	/// <para>
	/// This processor primarily handles "editor-only" fields.
	/// These fields exist in the Unity Editor, but not in compiled game files.
	/// Without this processing, those fields would have C# default values of zero.
	/// </para>
	/// <para>
	/// For most fields, this is just setting the field to the Unity default.
	/// However for some fields, there can be a calculation to recover an appropriate
	/// value for the field. For example, <see cref="ITransform.LocalEulerAnglesHint_C4"/>
	/// is set using <see cref="ITransform.LocalRotation_C4"/> with a Quaternion to
	/// Euler angle conversion. Similarly, <see cref="ITransform.RootOrder_C4"/> is
	/// calculated from <see cref="ITransform.Father_C4P"/> and <see cref="ITransform.Children_C4P"/>.
	/// </para>
	/// <para>
	/// Compiled game files can be identified from binary editor files by the 
	/// <see cref="TransferInstructionFlags.SerializeGameRelease"/> flag.
	/// However, those binary editor files are not commonly ripped with AssetRipper.
	/// More often, generated <see cref="ProcessedAssetCollection"/>s are given editor flags
	/// so as to exclude them from unnecessary processing. This is the default for
	/// <see cref="GameBundle.AddNewProcessedCollection(string, UnityVersion)"/>.
	/// </para>
	/// </summary>
	public class EditorFormatProcessor : IAssetProcessor
	{
		private ITagManager? tagManager;
		private readonly BundledAssetsExportMode bundledAssetsExportMode;
		private readonly PathProcessor propertyNameProcessor;
		private AnimationCache? currentAnimationCache;

		public EditorFormatProcessor(BundledAssetsExportMode bundledAssetsExportMode, IAssemblyManager assemblyManager)
		{
			this.bundledAssetsExportMode = bundledAssetsExportMode;
			propertyNameProcessor = new PathProcessor(assemblyManager);
		}

		public void Process(GameBundle gameBundle, UnityVersion projectVersion)
		{
			Logger.Info(LogCategory.Processing, "Editor Format Conversion");
			tagManager = gameBundle.FetchAssets().OfType<ITagManager>().FirstOrDefault();
			currentAnimationCache = AnimationCache.CreateCache(gameBundle);
			foreach (AssetCollection collection in gameBundle.FetchAssetCollections().Where(c => c.Flags.IsRelease()))
			{
				foreach (IUnityObjectBase asset in collection)
				{
					Convert(asset);
				}
			}
			currentAnimationCache = null;
			tagManager = null;
		}

		private void Convert(IUnityObjectBase asset)
		{
			switch (asset)
			{
				//ordered by approximate frequency
				case IGameObject gameObject:
					gameObject.ConvertToEditorFormat(tagManager);
					break;
				case ITransform transform:
					transform.ConvertToEditorFormat();
					break;
				case IRenderer renderer:
					renderer.ConvertToEditorFormat();
					break;
				case IMesh mesh:
					mesh.ConvertToEditorFormat();
					break;
				case ISpriteAtlas spriteAtlas:
					spriteAtlas.ConvertToEditorFormat();
					break;
				case IAnimationClip animationClip:
					AnimationClipConverter.Process(animationClip, currentAnimationCache!, propertyNameProcessor);
					break;
				case ITerrain terrain:
					terrain.ConvertToEditorFormat();
					break;
				case IAssetBundle assetBundle:
					SetOriginalPaths(assetBundle, bundledAssetsExportMode);
					break;
				case IGraphicsSettings graphicsSettings:
					graphicsSettings.ConvertToEditorFormat();
					break;
				case IQualitySettings qualitySettings:
					qualitySettings.ConvertToEditorFormat();
					break;
				case IPhysics2DSettings physics2DSettings:
					physics2DSettings.ConvertToEditorFormat();
					break;
				case ILightmapSettings lightmapSettings:
					lightmapSettings.ConvertToEditorFormat();
					break;
				case INavMeshSettings navMeshSettings:
					navMeshSettings.ConvertToEditorFormat();
					break;
				case IResourceManager resourceManager:
					SetOriginalPaths(resourceManager);
					break;
				case IPlayerSettings playerSettings:
					playerSettings.AllowUnsafeCode_C129 = true;
					break;
			}
		}

		private static void SetOriginalPaths(IResourceManager manager)
		{
			foreach (AccessPairBase<Utf8String, IPPtr_Object> kvp in manager.Container_C147)
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
					UndoPathLowercasing(asset);
				}
				else if (asset.OriginalPath.Length < resourcePath.Length)
				{
					// for paths like "Resources/inner/resources/extra/file" engine creates 2 resource entries
					// "inner/resources/extra/file" and "extra/file"
					asset.OriginalPath = resourcePath;
					UndoPathLowercasing(asset);
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
		private static void SetOriginalPaths(IAssetBundle bundle, BundledAssetsExportMode bundledAssetsExportMode)
		{
			string bundleName = bundle.GetAssetBundleName();
			string bundleDirectory = bundleName + DirectorySeparator;
			string directory = Path.Combine(AssetBundleFullPath, bundleName);
			foreach (AccessPairBase<Utf8String, IAssetInfo> kvp in bundle.Container_C142)
			{
				// skip shared bundle assets, because we need to export them in their bundle directory
				if (kvp.Value.Asset.FileID != 0)
				{
					continue;
				}

				IUnityObjectBase? asset = kvp.Value.Asset.TryGetAsset(bundle.Collection);
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
				UndoPathLowercasing(asset);
			}
		}

		/// <summary>
		/// During compilation, Unity often lowers all the characters in a path. This restores the proper capitalization for asset names.
		/// </summary>
		/// <param name="asset"></param>
		private static void UndoPathLowercasing(IUnityObjectBase asset)
		{
			string? assetName = (asset as IHasNameString)?.NameString;
			string? originalName = asset.OriginalName;
			if (assetName is not null
				&& originalName is not null
				&& assetName.Length == originalName.Length
				&& originalName == assetName.ToLowerInvariant())
			{
				asset.OriginalName = assetName;
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
					if (string.Equals(pathSection, AssetsKeyword, StringComparison.OrdinalIgnoreCase))
					{
						return string.Join(DirectorySeparator, new ArraySegment<string>(splitPath, i, splitPath.Length - i));
					}
				}
				return string.Empty;
			}
			else
			{
				return assetPath;
			}
		}

		private const string ResourcesKeyword = "Resources";
		private const string AssetBundleKeyword = "AssetBundles";
		private const string DirectorySeparator = "/";
		private const string AssetsDirectory = AssetsKeyword + DirectorySeparator;
		private const string ResourceFullPath = AssetsDirectory + ResourcesKeyword;
		//private const string AssetBundleFullPath = AssetsDirectory + AssetBundleKeyword;
		private const string AssetBundleFullPath = AssetsDirectory + "Asset_Bundles";
		private const string AssetsKeyword = "Assets";
	}
}
