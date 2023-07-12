using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated;

namespace AssetRipper.Tests;

internal static class AssetCreator
{
	public static T CreateAsset<T>(ClassIDType classID, UnityVersion version, Func<AssetInfo, T> factory) where T : IUnityObjectBase
	{
		return MakeCollection(version).CreateAsset<T>((int)classID, factory);
	}

	public static T CreateAsset<T>(ClassIDType classID, UnityVersion version) where T : IUnityObjectBase
	{
		return MakeCollection(version).CreateAsset((int)classID, (assetInfo) =>
		{
			return (T)AssetFactory.Create(assetInfo);
		});
	}

	private static ProcessedAssetCollection MakeCollection(UnityVersion version)
	{
		GameBundle gameBundle = new();
		ProcessedAssetCollection collection = gameBundle.AddNewProcessedCollection(nameof(AssetCreator), version);
		return collection;
	}
}
