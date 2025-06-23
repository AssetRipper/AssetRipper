using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.Assets.Tests;

public class FileResolutionTests
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

		using (Assert.EnterMultipleScope())
		{
			Assert.That(gameBundle.ResolveCollection(name1), Is.EqualTo(collection1));
			Assert.That(gameBundle.ResolveCollection(name2), Is.EqualTo(collection2));
			Assert.That(processedBundle.ResolveCollection(name1), Is.EqualTo(collection1));
			Assert.That(processedBundle.ResolveCollection(name2), Is.EqualTo(collection2));
		}
	}

	[Test]
	public void CollectionResolutionIsAbleToFindTheSecondFile()
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
		using (Assert.EnterMultipleScope())
		{
			Assert.That(gameBundle.ResolveCollection(name1), Is.EqualTo(collection1));
			Assert.That(gameBundle.ResolveCollection(name2), Is.EqualTo(collection2));
		}
	}

	[Test]
	public void CollectionResolutionIsAbleToFindUnityDefaultResourcesWithInconsistentUnderscores()
	{
		const string name = "unity_default_resources";
		GameBundle gameBundle = new();

		ProcessedAssetCollection collection = new ProcessedAssetCollection(gameBundle);
		collection.Name = name;

		Assert.That(gameBundle.ResolveCollection("library/unity default resources"), Is.EqualTo(collection));
	}

	[TestCase("unity default resources")]
	[TestCase("unity_default_resources")]
	[TestCase("unity editor resources")]
	[TestCase("unity builtin extra")]
	[TestCase("unity_builtin_extra")]
	public void CollectionResolutionIsAbleToFindEngineResourcess(string name)
	{
		GameBundle gameBundle = new();

		ProcessedAssetCollection collection = new ProcessedAssetCollection(gameBundle);
		collection.Name = name;
		using (Assert.EnterMultipleScope())
		{
			Assert.That(gameBundle.ResolveCollection(name), Is.EqualTo(collection));
			Assert.That(gameBundle.ResolveCollection($"library/{name}"), Is.EqualTo(collection));
			Assert.That(gameBundle.ResolveCollection($"resources/{name}"), Is.EqualTo(collection));
		}
	}

	[TestCase("unity default resources")]
	[TestCase("unity_default_resources")]
	[TestCase("unity editor resources")]
	[TestCase("unity builtin extra")]
	[TestCase("unity_builtin_extra")]
	public void CollectionResolutionIsAbleToFindEngineResourcesNested(string name)
	{
		GameBundle gameBundle = new();

		ProcessedBundle processedBundle = new();
		gameBundle.AddBundle(processedBundle);

		ProcessedAssetCollection collection = new ProcessedAssetCollection(processedBundle);
		collection.Name = name;
		using (Assert.EnterMultipleScope())
		{
			Assert.That(gameBundle.ResolveCollection(name), Is.EqualTo(collection));
			Assert.That(gameBundle.ResolveCollection($"library/{name}"), Is.EqualTo(collection));
			Assert.That(gameBundle.ResolveCollection($"resources/{name}"), Is.EqualTo(collection));
		}
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
		using (Assert.EnterMultipleScope())
		{
			Assert.That(gameBundle.ResolveResource(name1), Is.EqualTo(resource1));
			Assert.That(gameBundle.ResolveResource(name2), Is.EqualTo(resource2));
			Assert.That(processedBundle.ResolveResource(name1), Is.EqualTo(resource1));
			Assert.That(processedBundle.ResolveResource(name2), Is.EqualTo(resource2));
		}
	}

	[Test]
	public void ResourceResolutionIsAbleToFindTheSecondFile()
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
		using (Assert.EnterMultipleScope())
		{
			Assert.That(gameBundle.ResolveResource(name1), Is.EqualTo(resource1));
			Assert.That(gameBundle.ResolveResource(name2), Is.EqualTo(resource2));
		}
	}

	[Test]
	public void ResourceResolutionIsAbleToFindAnArchiveFile()
	{
		const string name = "archive:/name1";
		GameBundle gameBundle = new();

		ProcessedBundle processedBundle = new();
		gameBundle.AddBundle(processedBundle);

		ResourceFile resource = CreateNewResourceFile(name);
		processedBundle.AddResource(resource);

		Assert.That(gameBundle.ResolveResource(name), Is.EqualTo(resource));
	}

	[Test]
	public void ResourceResolutionIsAbleToFindFilesWithCapitalLetters()
	{
		const string name = "ResourceName.resS";
		GameBundle gameBundle = new();

		ProcessedBundle processedBundle = new();
		gameBundle.AddBundle(processedBundle);

		ResourceFile resource = CreateNewResourceFile(name);
		processedBundle.AddResource(resource);

		Assert.That(gameBundle.ResolveResource(name), Is.EqualTo(resource));
	}

	[Test]
	public void ResourceResolutionIsAbleToFindExternalFilesFromParentBundles()
	{
		const string resourceName = "resources.resource";
		GameBundle gameBundle = new();

		ProcessedBundle processedBundle = new();
		gameBundle.AddBundle(processedBundle);

		ResourceFile resource = CreateNewResourceFile(resourceName);
		gameBundle.ResourceProvider = new SingleResourceProvider(resource);

		Assert.That(processedBundle.ResolveResource(resourceName), Is.EqualTo(resource));
	}

	[Test]
	public void ResourceResolutionIsAbleToFindExternalFilesFromGameBundles()
	{
		const string resourceName = "resources.resource";
		GameBundle gameBundle = new();

		ResourceFile resource = CreateNewResourceFile(resourceName);
		gameBundle.ResourceProvider = new SingleResourceProvider(resource);

		Assert.That(gameBundle.ResolveResource(resourceName), Is.EqualTo(resource));
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
