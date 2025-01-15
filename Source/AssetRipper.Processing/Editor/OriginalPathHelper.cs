using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Processing.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_142;
using AssetRipper.SourceGenerated.Classes.ClassID_147;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.AssetInfo;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Object;

namespace AssetRipper.Processing.Editor;

internal static class OriginalPathHelper
{
	private const string ResourcesKeyword = "Resources";
	private const string DirectorySeparator = "/";
	private const string AssetsDirectory = AssetsKeyword + DirectorySeparator;
	private const string ResourceFullPath = AssetsDirectory + ResourcesKeyword;
	private const string AssetBundleFullPath = AssetsDirectory + "Asset_Bundles";
	private const string AssetsKeyword = "Assets";

	internal static void SetOriginalPaths(IResourceManager manager)
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
	/// TODO: Asset bundles usually contain more assets than listed in <see cref="IAssetBundle.Container"/>. 
	/// Need to export them in AssetBundleFullPath directory if <see cref="m_BundledAssetsExportMode"/> is <see cref="BundledAssetsExportMode.GroupByBundleName"/>.
	/// Or maybe remove that mode entirely. It has dubious utility.
	/// </remarks>
	/// <param name="bundle"></param>
	/// <exception cref="Exception"></exception>
	internal static void SetOriginalPaths(IAssetBundle bundle, BundledAssetsExportMode bundledAssetsExportMode)
	{
		string bundleName = bundle.GetAssetBundleName();
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

			string assetPath = EnsurePathNotRooted(kvp.Key.String);
			if (string.IsNullOrEmpty(assetPath))
			{
				continue;
			}

			switch (bundledAssetsExportMode)
			{
				case BundledAssetsExportMode.DirectExport:
					asset.OriginalPath = EnsureStartsWithAssets(assetPath);
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
				case BundledAssetsExportMode.GroupByAssetType:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(bundledAssetsExportMode), $"Invalid {nameof(BundledAssetsExportMode)} : {bundledAssetsExportMode}");
			}
			UndoPathLowercasing(asset);
			SetOverridePathIfShader(asset);
		}
	}

	internal static string EnsurePathNotRooted(string assetPath)
	{
		if (Path.IsPathRooted(assetPath))
		{
			string[] splitPath = assetPath.Split('/');
			for (int i = 0; i < splitPath.Length; i++)
			{
				string pathSection = splitPath[i];
				if (string.Equals(pathSection, AssetsKeyword, StringComparison.OrdinalIgnoreCase))
				{
					return string.Join(DirectorySeparator, new ReadOnlySpan<string?>(splitPath, i, splitPath.Length - i));
				}
			}
			return string.Empty;
		}
		else
		{
			return assetPath;
		}
	}

	internal static string EnsureStartsWithAssets(string assetPath)
	{
		if (assetPath.StartsWith(AssetsDirectory, StringComparison.Ordinal))
		{
			return assetPath;
		}
		else if (assetPath.StartsWith(AssetsDirectory, StringComparison.OrdinalIgnoreCase))
		{
			return $"{AssetsDirectory}{assetPath.AsSpan(AssetsDirectory.Length)}";
		}
		else
		{
			return AssetsDirectory + assetPath;
		}
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
			&& originalName == assetName.ToLowerInvariant())
		{
			asset.OriginalName = assetName;
		}
	}

	private static void SetOverridePathIfShader(IUnityObjectBase asset)
	{
		if (asset is IShader shader)
		{
			shader.OverrideDirectory ??= shader.OriginalDirectory;
			shader.OverrideName ??= shader.OriginalName;
			shader.OverrideExtension ??= shader.OriginalExtension;
		}
	}
}
