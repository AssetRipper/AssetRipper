using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated;
using AssetRipper.VersionUtilities;

namespace AssetRipper.Tests;

internal static class AssetCreator
{
	public static T CreateAsset<T>(ClassIDType classID, UnityVersion version, Func<AssetInfo, T> factory) where T : IUnityObjectBase
	{
		return MakeCollection(version).CreateAsset<T>((int)classID, factory);
	}

	public static T CreateAsset<T>(ClassIDType classID, UnityVersion version, Func<UnityVersion, AssetInfo, T> factory) where T : IUnityObjectBase
	{
		return MakeCollection(version).CreateAsset<T>((int)classID, factory);
	}

	public static T CreateAsset<T>(ClassIDType classID, UnityVersion version) where T : IUnityObjectBase
	{
		return MakeCollection(version).CreateAsset<T>((int)classID, CreateAssetInternal<T>);
	}

	private static ProcessedAssetCollection MakeCollection(UnityVersion version)
	{
		GameBundle gameBundle = new();
		ProcessedAssetCollection collection = gameBundle.AddNewProcessedCollection(nameof(AssetCreator), version);
		return collection;
	}

	private static T CreateAssetInternal<T>(UnityVersion version, AssetInfo assetInfo) where T : IUnityObjectBase
	{
		return (T)AssetFactory.CreateAsset(version, assetInfo);
	}
}
