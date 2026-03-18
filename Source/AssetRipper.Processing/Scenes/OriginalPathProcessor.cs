using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Processing.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_142;
using AssetRipper.SourceGenerated.Classes.ClassID_147;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.AssetInfo;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Object;

namespace AssetRipper.Processing.Scenes;

public sealed class OriginalPathProcessor : IAssetProcessor
{
	private const string ResourcesKeyword = "Resources";
	private const string DirectorySeparator = "/";
	private const string AssetsDirectory = AssetsKeyword + DirectorySeparator;
	private const string ResourceFullPath = AssetsDirectory + ResourcesKeyword;
	private const string AssetBundleFullPath = AssetsDirectory + "AssetBundles";
	private const string AssetsKeyword = "Assets";

	private readonly BundledAssetsExportMode bundledAssetsExportMode;

	public OriginalPathProcessor(BundledAssetsExportMode bundledAssetsExportMode)
	{
		this.bundledAssetsExportMode = bundledAssetsExportMode;
	}

	public void Process(GameData gameData)
	{
		Dictionary<AssetCollection, (string BundleName, IAssetBundle BundleAsset)> dictionary = [];
		foreach (IUnityObjectBase asset in gameData.GameBundle.FetchAssets())
		{
			switch (asset)
			{
				case IResourceManager resourceManager:
					SetOriginalPaths(resourceManager);
					break;
				case IAssetBundle assetBundle:
					SetOriginalPaths(assetBundle, bundledAssetsExportMode);
					if (bundledAssetsExportMode is BundledAssetsExportMode.GroupByBundleName)
					{
						string assetBundleName = EnsureDoesNotEndWithBundleExtension(assetBundle.GetAssetBundleName());
						if (asset.Collection.Bundle is not GameBundle)
						{
							foreach (AssetCollection collection in asset.Collection.Bundle.Collections)
							{
								dictionary[collection] = (assetBundleName, assetBundle);
							}
						}
					}
					break;
			}
		}

		foreach ((AssetCollection collection, (string BundleName, IAssetBundle BundleAsset)) in dictionary)
		{
			foreach (IUnityObjectBase asset in collection)
			{
				asset.OriginalDirectory ??= Path.Join(AssetBundleFullPath, BundleName, asset.ClassName);
			}
		}
	}

	private static void SetOriginalPaths(IResourceManager manager)
	{
		foreach (AccessPairBase<Utf8String, IPPtr_Object> kvp in manager.Container)
		{
			IUnityObjectBase? asset = kvp.Value.TryGetAsset(manager.Collection);
			if (asset is null)
			{
				continue;
			}

			string resourcePath = Path.Join(ResourceFullPath, kvp.Key.String);
			if (asset.OriginalPath is null)
			{
				asset.OriginalPath = resourcePath;
				UndoPathLowercasing(asset);
				SetOverridePathIfShader(asset);
			}
			else if (asset.OriginalPath.Length < resourcePath.Length)
			{
				// for paths like "Resources/inner/resources/extra/file" engine creates 2 resource entries
				// "inner/resources/extra/file" and "extra/file"
				asset.OriginalPath = resourcePath;
				UndoPathLowercasing(asset);
				SetOverridePathIfShader(asset);
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// Asset bundles usually contain more assets than listed in <see cref="IAssetBundle.Container"/>. 
	/// We need to export them in AssetBundleFullPath directory if <see cref="m_BundledAssetsExportMode"/> is <see cref="BundledAssetsExportMode.GroupByBundleName"/>.
	/// That is done in a separate function.
	/// </remarks>
	/// <param name="bundle"></param>
	/// <exception cref="Exception"></exception>
	private static void SetOriginalPaths(IAssetBundle bundle, BundledAssetsExportMode bundledAssetsExportMode)
	{
		string bundleName = EnsureDoesNotEndWithBundleExtension(bundle.GetAssetBundleName());
		string bundleDirectory = bundleName + DirectorySeparator;
		string directory = Path.Join(AssetBundleFullPath, bundleName);
		foreach (AccessPairBase<Utf8String, IAssetInfo> kvp in bundle.Container)
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

			string assetPath = OriginalPathHelper.EnsurePathNotRooted(kvp.Key.String);
			if (string.IsNullOrEmpty(assetPath))
			{
				continue;
			}

			switch (bundledAssetsExportMode)
			{
				case BundledAssetsExportMode.DirectExport:
					asset.OriginalPath = OriginalPathHelper.EnsureStartsWithAssets(assetPath);
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
					asset.OriginalPath = Path.Join(directory, assetPath);
					break;
			}
			UndoPathLowercasing(asset);
			SetOverridePathIfShader(asset);
		}
	}

	private static string EnsureDoesNotEndWithBundleExtension(string path)
	{
		// We need to remove the .bundle extension if present. Unity exhibits weird behavior if a folder name ends with ".bundle".
		// On 2019.4.3 for example, materials contained in such a folder (or any subfolder) will not preview in the editor and cannot be viewed in the inspector.
		// I could not find any official documentation on this behavior, but it seems to be for packaging native code on Mac and iOS.
		// https://docs.unity3d.com/2017.3/Documentation/Manual/PluginsForDesktop.html

		const string BundleExtension = ".bundle";
		if (path.EndsWith(BundleExtension, StringComparison.OrdinalIgnoreCase))
		{
			return path[..^BundleExtension.Length];
		}
		return path;
	}

	/// <summary>
	/// During compilation, Unity often lowers all the characters in a path. This restores the proper capitalization for asset names.
	/// </summary>
	/// <param name="asset"></param>
	private static void UndoPathLowercasing(IUnityObjectBase asset)
	{
		string? assetName = (asset as INamed)?.Name;
		string? originalName = asset.OriginalName;
		if (assetName is not null
			&& originalName is not null
			&& assetName.Length == originalName.Length
			&& originalName.Equals(assetName, StringComparison.OrdinalIgnoreCase))
		{
			asset.OriginalName = assetName;
		}
	}

	private static void SetOverridePathIfShader(IUnityObjectBase asset)
	{
		// Original name is prioritized below the asset name, so we need to set the override path.
		// Otherwise, the shader will be exported with the wrong name.
		if (asset is IShader shader)
		{
			shader.OverrideDirectory ??= shader.OriginalDirectory;
			shader.OverrideName ??= shader.OriginalName;
			shader.OverrideExtension ??= shader.OriginalExtension;
		}
	}
}
