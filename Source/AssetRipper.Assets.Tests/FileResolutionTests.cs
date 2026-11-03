using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.Assets.Tests;

public class FileResolutionTests
{
	[Test]
	public async Task CollectionResolutionWorksAnywhereInTheHierarchy()
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

		{
			await Assert.That(gameBundle.ResolveCollection(name1)).IsEqualTo(collection1);
			await Assert.That(gameBundle.ResolveCollection(name2)).IsEqualTo(collection2);
			await Assert.That(processedBundle.ResolveCollection(name1)).IsEqualTo(collection1);
			await Assert.That(processedBundle.ResolveCollection(name2)).IsEqualTo(collection2);
		}
	}

	[Test]
	public async Task CollectionResolutionIsAbleToFindTheSecondFile()
	{
		const string name1 = "name1";
		const string name2 = "name2";
		GameBundle gameBundle = new();

		ProcessedBundle processedBundle1 = new();
		gameBundle.AddBundle(processedBundle1);

		ProcessedAssetCollection collection1 = new ProcessedAssetCollection(processedBundle1);
		collection1.Name = name1;

		ProcessedBundle processedBundle2 = new();
		gameBundle.AddBundle(processedBundle2);

		ProcessedAssetCollection collection2 = new ProcessedAssetCollection(processedBundle2);
		collection2.Name = name2;

		{
			await Assert.That(gameBundle.ResolveCollection(name1)).IsEqualTo(collection1);
			await Assert.That(gameBundle.ResolveCollection(name2)).IsEqualTo(collection2);
		}
	}

	[Test]
	public async Task CollectionResolutionIsAbleToFindUnityDefaultResourcesWithInconsistentUnderscores()
	{
		const string name = "unity_default_resources";
		GameBundle gameBundle = new();

		ProcessedAssetCollection collection = new ProcessedAssetCollection(gameBundle);
		collection.Name = name;

		await Assert.That(gameBundle.ResolveCollection("library/unity default resources")).IsEqualTo(collection);
	}

	[Test]
	[Arguments("unity default resources")]
	[Arguments("unity_default_resources")]
	[Arguments("unity editor resources")]
	[Arguments("unity builtin extra")]
	[Arguments("unity_builtin_extra")]
	public async Task CollectionResolutionIsAbleToFindEngineResourcess(string name)
	{
		GameBundle gameBundle = new();

		ProcessedAssetCollection collection = new ProcessedAssetCollection(gameBundle);
		collection.Name = name;

		{
			await Assert.That(gameBundle.ResolveCollection(name)).IsEqualTo(collection);
			await Assert.That(gameBundle.ResolveCollection($"library/{name}")).IsEqualTo(collection);
			await Assert.That(gameBundle.ResolveCollection($"resources/{name}")).IsEqualTo(collection);
		}
	}

	[Test]
	[Arguments("unity default resources")]
	[Arguments("unity_default_resources")]
	[Arguments("unity editor resources")]
	[Arguments("unity builtin extra")]
	[Arguments("unity_builtin_extra")]
	public async Task CollectionResolutionIsAbleToFindEngineResourcesNested(string name)
	{
		GameBundle gameBundle = new();

		ProcessedBundle processedBundle = new();
		gameBundle.AddBundle(processedBundle);

		ProcessedAssetCollection collection = new ProcessedAssetCollection(processedBundle);
		collection.Name = name;

		{
			await Assert.That(gameBundle.ResolveCollection(name)).IsEqualTo(collection);
			await Assert.That(gameBundle.ResolveCollection($"library/{name}")).IsEqualTo(collection);
			await Assert.That(gameBundle.ResolveCollection($"resources/{name}")).IsEqualTo(collection);
		}
	}

	[Test]
	public async Task ResourceResolutionWorksAnywhereInTheHierarchy()
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

		{
			await Assert.That(gameBundle.ResolveResource(name1)).IsEqualTo(resource1);
			await Assert.That(gameBundle.ResolveResource(name2)).IsEqualTo(resource2);
			await Assert.That(processedBundle.ResolveResource(name1)).IsEqualTo(resource1);
			await Assert.That(processedBundle.ResolveResource(name2)).IsEqualTo(resource2);
		}
	}

	[Test]
	public async Task ResourceResolutionIsAbleToFindTheSecondFile()
	{
		const string name1 = "name1";
		const string name2 = "name2";
		GameBundle gameBundle = new();

		ProcessedBundle processedBundle1 = new();
		gameBundle.AddBundle(processedBundle1);

		ResourceFile resource1 = CreateNewResourceFile(name1);
		processedBundle1.AddResource(resource1);

		ProcessedBundle processedBundle2 = new();
		gameBundle.AddBundle(processedBundle2);

		ResourceFile resource2 = CreateNewResourceFile(name2);
		processedBundle2.AddResource(resource2);

		{
			await Assert.That(gameBundle.ResolveResource(name1)).IsEqualTo(resource1);
			await Assert.That(gameBundle.ResolveResource(name2)).IsEqualTo(resource2);
		}
	}

	[Test]
	public async Task ResourceResolutionIsAbleToFindAnArchiveFile()
	{
		const string name = "archive:/name1";
		GameBundle gameBundle = new();

		ProcessedBundle processedBundle = new();
		gameBundle.AddBundle(processedBundle);

		ResourceFile resource = CreateNewResourceFile(name);
		processedBundle.AddResource(resource);

		await Assert.That(gameBundle.ResolveResource(name)).IsEqualTo(resource);
	}

	[Test]
	public async Task ResourceResolutionIsAbleToFindFilesWithCapitalLetters()
	{
		const string name = "ResourceName.resS";
		GameBundle gameBundle = new();

		ProcessedBundle processedBundle = new();
		gameBundle.AddBundle(processedBundle);

		ResourceFile resource = CreateNewResourceFile(name);
		processedBundle.AddResource(resource);

		await Assert.That(gameBundle.ResolveResource(name)).IsEqualTo(resource);
	}

	[Test]
	public async Task ResourceResolutionIsAbleToFindExternalFilesFromParentBundles()
	{
		const string resourceName = "resources.resource";
		GameBundle gameBundle = new();

		ProcessedBundle processedBundle = new();
		gameBundle.AddBundle(processedBundle);

		ResourceFile resource = CreateNewResourceFile(resourceName);
		gameBundle.ResourceProvider = new SingleResourceProvider(resource);

		await Assert.That(processedBundle.ResolveResource(resourceName)).IsEqualTo(resource);
	}

	[Test]
	public async Task ResourceResolutionIsAbleToFindExternalFilesFromGameBundles()
	{
		const string resourceName = "resources.resource";
		GameBundle gameBundle = new();

		ResourceFile resource = CreateNewResourceFile(resourceName);
		gameBundle.ResourceProvider = new SingleResourceProvider(resource);

		await Assert.That(gameBundle.ResolveResource(resourceName)).IsEqualTo(resource);
	}

	private sealed record class SingleResourceProvider(ResourceFile Resource) : IResourceProvider
	{
		public ResourceFile? FindResource(string identifier)
		{
			string fixedName = SpecialFileNames.FixResourcePath(identifier);
			return fixedName == Resource.NameFixed ? Resource : null;
		}
	}

	private static ResourceFile CreateNewResourceFile(string name) => new ResourceFile(SmartStream.CreateMemory(), name, name);
}