using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.Assets.Tests;

public class ResolutionTests
{
	[Test]
	public void CollectionResolutionWorksAnywhereInTheHierarchy()
	{
		const string name1 = "name1";
		const string name2 = "name2";
		GameBundle gameBundle = new();

		ProcessedAssetCollection collection1 = new ProcessedAssetCollection(gameBundle);
		collection1.Name = name1;

		ProcessedBundle processedBundle = new();
		gameBundle.AddBundle(processedBundle);

		ProcessedAssetCollection collection2 = new ProcessedAssetCollection(processedBundle);
		collection2.Name = name2;

		Assert.Multiple(() =>
		{
			Assert.That(gameBundle.ResolveCollection(name1), Is.EqualTo(collection1));
			Assert.That(gameBundle.ResolveCollection(name2), Is.EqualTo(collection2));
			Assert.That(processedBundle.ResolveCollection(name1), Is.EqualTo(collection1));
			Assert.That(processedBundle.ResolveCollection(name2), Is.EqualTo(collection2));
		});
	}

	[Test]
	public void ResourceResolutionWorksAnywhereInTheHierarchy()
	{
		const string name1 = "name1";
		const string name2 = "name2";
		GameBundle gameBundle = new();

		ResourceFile resource1 = CreateNewResourceFile(name1);
		gameBundle.AddResource(resource1);

		ProcessedBundle processedBundle = new();
		gameBundle.AddBundle(processedBundle);

		ResourceFile resource2 = CreateNewResourceFile(name2);
		processedBundle.AddResource(resource2);

		Assert.Multiple(() =>
		{
			Assert.That(gameBundle.ResolveResource(name1), Is.EqualTo(resource1));
			Assert.That(gameBundle.ResolveResource(name2), Is.EqualTo(resource2));
			Assert.That(processedBundle.ResolveResource(name1), Is.EqualTo(resource1));
			Assert.That(processedBundle.ResolveResource(name2), Is.EqualTo(resource2));
		});

		static ResourceFile CreateNewResourceFile(string name) => new ResourceFile(SmartStream.CreateMemory(), name, name);
	}
}
