using AssetRipper.Export.UnityProjects.EngineAssets;
using AssetRipper.Import.Logging;
using AssetRipper.Mining.PredefinedAssets;
using FileAssetType = AssetRipper.IO.Files.AssetType;
using MiningAssetType = AssetRipper.Mining.PredefinedAssets.AssetType;
using MiningObject = AssetRipper.Mining.PredefinedAssets.Object;

namespace AssetRipper.Processing.Extended.UnityPackages;

/// <summary>
/// Builds a <see cref="PredefinedAssetCache"/> from mined package data, mirroring how
/// the engine-resource constructor builds one from <see cref="EngineResourceData"/>.
/// </summary>
public static class PackageAssetCacheFactory
{
	public static PredefinedAssetCache Create(UnityPackageData package)
	{
		PredefinedAssetCache cache = new();
		int added = 0;
		foreach ((MiningObject minedObject, PPtr pointer) in package.Assets)
		{
			if (cache.TryAdd(minedObject, pointer.FileID, pointer.Guid, Convert(pointer.Type)))
			{
				added++;
			}
		}
		Logger.Info(LogCategory.Export, $"Package '{package.Name}' {package.Version}: cached {added} of {package.Assets.Count} assets.");
		return cache;
	}

	/// <summary>
	/// The two enums share member names but are distinct types. Map by name rather than
	/// assuming the numeric values line up.
	/// </summary>
	private static FileAssetType Convert(MiningAssetType type) => type switch
	{
		MiningAssetType.Internal => FileAssetType.Internal,
		MiningAssetType.Cached => FileAssetType.Cached,
		MiningAssetType.Serialized => FileAssetType.Serialized,
		MiningAssetType.Meta => FileAssetType.Meta,
		_ => FileAssetType.Serialized,
	};
}
