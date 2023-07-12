using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Exceptions;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Classes.ClassID_1;

namespace AssetRipper.Assets.Tests;

public class AssetDeletionTests
{
	[Test]
	public void DeletingAnAssetFromAnotherCollectionThrows()
	{
		UnityVersion projectVersion = new UnityVersion(2022, 1);
		GameBundle gameBundle = new();
		ProcessedAssetCollection collection1 = gameBundle.AddNewProcessedCollection("Collection 1", projectVersion);
		ProcessedAssetCollection collection2 = gameBundle.AddNewProcessedCollection("Collection 2", projectVersion);
		IGameObject gameObject = collection1.CreateAsset(1, GameObject.Create);

		Assert.Throws<ArgumentException>(() => collection2.DeleteAsset(gameObject, false));
		Assert.Throws<ArgumentException>(() => collection2.DeleteAsset(gameObject, true));
		collection1.DeleteAsset(gameObject, true);
	}

	[Test]
	public void TestDeletingWithTheIntentionToThrow()
	{
		ProcessedAssetCollection collection = new GameBundle().AddNewProcessedCollection("Collection", new UnityVersion(2022, 1));
		IGameObject gameObject = collection.CreateAsset(1, GameObject.Create);

		Assert.That(collection.GetAsset(gameObject.PathID), Is.EqualTo(gameObject));

		gameObject.Delete(true);

		Assert.Multiple(() =>
		{
			Assert.DoesNotThrow(() => collection.DeleteAsset(gameObject, true));
			Assert.Throws<ArgumentException>(() => collection.DeleteAsset(gameObject, false));
		});

		Assert.Multiple(() =>
		{
			Assert.Throws<AssetDeletedException>(() => collection.TryGetAsset(gameObject.PathID));
			Assert.Throws<AssetDeletedException>(() => collection.GetAsset(gameObject.PathID));
		});
	}

	[Test]
	public void TestDeletingWithTheIntentionNotToThrow()
	{
		ProcessedAssetCollection collection = new GameBundle().AddNewProcessedCollection("Collection", new UnityVersion(2022, 1));
		IGameObject gameObject = collection.CreateAsset(1, GameObject.Create);

		Assert.That(collection.GetAsset(gameObject.PathID), Is.EqualTo(gameObject));

		gameObject.Delete(false);

		Assert.Multiple(() =>
		{
			Assert.DoesNotThrow(() => collection.DeleteAsset(gameObject, false));
			Assert.Throws<ArgumentException>(() => collection.DeleteAsset(gameObject, true));
		});

		Assert.Multiple(() =>
		{
			Assert.DoesNotThrow(() => collection.TryGetAsset(gameObject.PathID));
			Assert.Throws<ArgumentException>(() => collection.GetAsset(gameObject.PathID));
		});

		Assert.That(collection.TryGetAsset(gameObject.PathID), Is.EqualTo(null));
	}

	[Test]
	public void DeletingANonExistantAssetThrows()
	{
		ProcessedAssetCollection collection = new GameBundle().AddNewProcessedCollection("Collection", new UnityVersion(2022, 1));
		
		Assert.Multiple(() =>
		{
			Assert.Throws<ArgumentException>(() => collection.DeleteAsset(0, true));
			Assert.Throws<ArgumentException>(() => collection.DeleteAsset(1, true));
		});
	}
}
