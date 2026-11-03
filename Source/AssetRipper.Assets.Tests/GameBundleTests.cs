using AssetRipper.Assets.Bundles;
using AssetRipper.Primitives;

namespace AssetRipper.Assets.Tests;

public class GameBundleTests
{
	[Test]
	public async Task HasAnyAssetCollections_ReturnsFalse()
	{
		GameBundle gameBundle = new();
		await Assert.That(gameBundle.HasAnyAssetCollections()).IsFalse();
	}

	[Test]
	public async Task AddNewProcessedCollection_AddsNewProcessedCollection()
	{
		GameBundle gameBundle = new();
		string name = "testName";
		UnityVersion version = UnityVersion.Parse("10.3.1f1");
		Collections.ProcessedAssetCollection processedCollection = gameBundle.AddNewProcessedCollection(name, version);

		{
			await Assert.That(processedCollection.Name).IsEqualTo(name);
			await Assert.That(processedCollection.Version).IsEqualTo(version);
		}
		await Assert.That(gameBundle.FetchAssetCollections()).Contains(processedCollection);
	}

	[Test]
	public async Task GetMaxUnityVersion_ReturnsMaxUnityVersion()
	{
		GameBundle gameBundle = new();
		gameBundle.AddNewProcessedCollection("test", UnityVersion.Parse("1.0.0f1"));
		gameBundle.AddNewProcessedCollection("test2", UnityVersion.Parse("2.0.10f3"));
		gameBundle.AddNewProcessedCollection("test3", UnityVersion.Parse("3.0.0f0"));
		await Assert.That(gameBundle.GetMaxUnityVersion()).IsEqualTo(UnityVersion.Parse("3.0.0f0"));
	}
}