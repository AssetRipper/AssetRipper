using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.Primitives;

namespace AssetRipper.Assets.Tests;

public class AssetResolutionTests
{
	[Test]
	public async Task ResolvingNullObjects()
	{
		ProcessedAssetCollection collection = Create();
		SealedNullObject asset = collection.CreateAsset(-1, (assetInfo) => new SealedNullObject(assetInfo));

		{
			//NullObject is not a real asset, so we should not be able to get it under normal conditions.
			await Assert.That(collection.TryGetAsset(asset.PathID)).IsNull();
			await Assert.That(collection.TryGetAsset(0, asset.PathID)).IsNull();
			await Assert.That(collection.TryGetAsset<IUnityObjectBase>(asset.PathID)).IsNull();
			await Assert.That(collection.TryGetAsset<IUnityObjectBase>(0, asset.PathID)).IsNull();

			//We are explicitly looking for a NullObject, so we should get it.
			await Assert.That(collection.TryGetAsset<NullObject>(asset.PathID)).IsEqualTo(asset);
			await Assert.That(collection.TryGetAsset<NullObject>(0, asset.PathID)).IsEqualTo(asset);
			await Assert.That(collection.TryGetAsset<SealedNullObject>(asset.PathID)).IsEqualTo(asset);
			await Assert.That(collection.TryGetAsset<SealedNullObject>(0, asset.PathID)).IsEqualTo(asset);
		}
	}

	private static ProcessedAssetCollection Create()
	{
		return new GameBundle().AddNewProcessedCollection(nameof(Create), new UnityVersion(2017));
	}

	private sealed class SealedNullObject : NullObject
	{
		public SealedNullObject(AssetInfo assetInfo) : base(assetInfo)
		{
		}
	}
}