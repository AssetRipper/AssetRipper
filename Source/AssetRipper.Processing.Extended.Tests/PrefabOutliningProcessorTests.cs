using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Primitives;
using AssetRipper.Processing.Extended.Deduplication;
using AssetRipper.Processing.Extended.Outlining;
using AssetRipper.Processing.Prefabs;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing.Extended.Tests;

public class PrefabOutliningProcessorTests
{
	private static readonly UnityVersion Version = new(2020, 1);

	private static IGameObject CreateRoot(ProcessedAssetCollection collection, string name)
	{
		IGameObject gameObject = collection.CreateGameObject();
		gameObject.Name = name;
		ITransform transform = collection.CreateTransform();
		transform.InitializeDefault();
		transform.GameObject_C4P = gameObject;
		gameObject.AddComponent(ClassIDType.Transform, transform);
		return gameObject;
	}

	/// <summary>
	/// Mirrors PrefabProcessor: every generated hierarchy lands in ONE shared collection,
	/// while the roots come from wherever the game put them.
	/// </summary>
	private static PrefabHierarchyObject CreateHierarchy(ProcessedAssetCollection sharedHierarchyCollection, IGameObject root)
	{
		IPrefabInstance prefab = sharedHierarchyCollection.CreatePrefabInstance();
		PrefabHierarchyObject hierarchy = sharedHierarchyCollection.CreateAsset(-1, (assetInfo) => new PrefabHierarchyObject(assetInfo, root, prefab));
		hierarchy.GameObjects.Add(root);
		return hierarchy;
	}

	[Test]
	public void CollapsesIdenticalHierarchiesWhoseRootsComeFromDifferentCollections()
	{
		// Regression: an earlier version grouped by the hierarchy object's own collection.
		// PrefabProcessor puts them all in one collection, so that guard silently disabled
		// the entire feature — every real run reported zero.
		GameBundle bundle = new();
		ProcessedAssetCollection sourceA = bundle.AddNewProcessedCollection("aaa", Version);
		ProcessedAssetCollection sourceB = bundle.AddNewProcessedCollection("bbb", Version);
		ProcessedAssetCollection hierarchies = bundle.AddNewProcessedCollection("Prefab Hierarchies", Version);

		PrefabHierarchyObject first = CreateHierarchy(hierarchies, CreateRoot(sourceA, "Card"));
		PrefabHierarchyObject second = CreateHierarchy(hierarchies, CreateRoot(sourceB, "Card"));

		DeduplicationMap map = new();
		new PrefabOutliningProcessor(map).Process(new GameData(bundle, Version, null!, null));

		Assert.That(map.TryGetCanonical(second.Root, out IUnityObjectBase? canonicalRoot), Is.True);
		Assert.That(canonicalRoot, Is.SameAs(first.Root));
		Assert.That(map.TryGetCanonical(first.Root, out _), Is.False);
	}

	[Test]
	public void LeavesStructurallyDifferentHierarchiesAlone()
	{
		GameBundle bundle = new();
		ProcessedAssetCollection sourceA = bundle.AddNewProcessedCollection("aaa", Version);
		ProcessedAssetCollection sourceB = bundle.AddNewProcessedCollection("bbb", Version);
		ProcessedAssetCollection hierarchies = bundle.AddNewProcessedCollection("Prefab Hierarchies", Version);

		CreateHierarchy(hierarchies, CreateRoot(sourceA, "Card"));
		CreateHierarchy(hierarchies, CreateRoot(sourceB, "Relic"));

		DeduplicationMap map = new();
		new PrefabOutliningProcessor(map).Process(new GameData(bundle, Version, null!, null));

		Assert.That(map.Count, Is.Zero);
	}

	[Test]
	public void LeavesHierarchiesFromOneSourceCollectionAlone()
	{
		GameBundle bundle = new();
		ProcessedAssetCollection source = bundle.AddNewProcessedCollection("aaa", Version);
		ProcessedAssetCollection hierarchies = bundle.AddNewProcessedCollection("Prefab Hierarchies", Version);

		CreateHierarchy(hierarchies, CreateRoot(source, "Card"));
		CreateHierarchy(hierarchies, CreateRoot(source, "Card"));

		DeduplicationMap map = new();
		new PrefabOutliningProcessor(map).Process(new GameData(bundle, Version, null!, null));

		Assert.That(map.Count, Is.Zero);
	}
}
