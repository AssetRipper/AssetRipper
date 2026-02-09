using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.Primitives;

namespace AssetRipper.Assets.Tests;

public class AssetResolutionTests
{
	[Test]
	public void ResolvingNullObjects()
	{
		ProcessedAssetCollection collection = Create();
		SealedNullObject asset = collection.CreateAsset(-1, (assetInfo) => new SealedNullObject(assetInfo));

		using (Assert.EnterMultipleScope())
		{
			//NullObject is not a real asset, so we should not be able to get it under normal conditions.
			Assert.That(collection.TryGetAsset(asset.PathID), Is.Null);
			Assert.That(collection.TryGetAsset(0, asset.PathID), Is.Null);
			Assert.That(collection.TryGetAsset<IUnityObjectBase>(asset.PathID), Is.Null);
			Assert.That(collection.TryGetAsset<IUnityObjectBase>(0, asset.PathID), Is.Null);

			//We are explicitly looking for a NullObject, so we should get it.
			Assert.That(collection.TryGetAsset<NullObject>(asset.PathID), Is.EqualTo(asset));
			Assert.That(collection.TryGetAsset<NullObject>(0, asset.PathID), Is.EqualTo(asset));
			Assert.That(collection.TryGetAsset<SealedNullObject>(asset.PathID), Is.EqualTo(asset));
			Assert.That(collection.TryGetAsset<SealedNullObject>(0, asset.PathID), Is.EqualTo(asset));
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
