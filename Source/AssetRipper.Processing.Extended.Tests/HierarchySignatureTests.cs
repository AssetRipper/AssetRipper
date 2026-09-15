using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Primitives;
using AssetRipper.Processing.Extended.Outlining;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing.Extended.Tests;

public class HierarchySignatureTests
{
	private static readonly UnityVersion Version = new(2020, 1);

	private static IGameObject CreateGameObject(ProcessedAssetCollection collection, string name, IGameObject? parent = null)
	{
		IGameObject gameObject = collection.CreateGameObject();
		gameObject.Name = name;
		ITransform transform = collection.CreateTransform();
		transform.InitializeDefault();
		transform.GameObject_C4P = gameObject;
		gameObject.AddComponent(ClassIDType.Transform, transform);
		if (parent is not null)
		{
			ITransform parentTransform = parent.GetComponent<ITransform>();
			transform.Father_C4P = parentTransform;
			parentTransform.Children_C4P.Add(transform);
		}
		return gameObject;
	}

	private static IGameObject BuildTree(ProcessedAssetCollection collection, string rootName, params string[] childNames)
	{
		IGameObject root = CreateGameObject(collection, rootName);
		foreach (string childName in childNames)
		{
			CreateGameObject(collection, childName, root);
		}
		return root;
	}

	[Test]
	public void IdenticalStructuresShareASignature()
	{
		GameBundle bundle = new();
		ProcessedAssetCollection a = bundle.AddNewProcessedCollection("a", Version);
		ProcessedAssetCollection b = bundle.AddNewProcessedCollection("b", Version);

		string first = HierarchySignature.Compute(BuildTree(a, "Root", "Left", "Right"));
		string second = HierarchySignature.Compute(BuildTree(b, "Root", "Left", "Right"));

		Assert.That(first, Is.EqualTo(second));
	}

	[Test]
	public void DifferentChildNamesChangeTheSignature()
	{
		GameBundle bundle = new();
		ProcessedAssetCollection a = bundle.AddNewProcessedCollection("a", Version);

		string first = HierarchySignature.Compute(BuildTree(a, "Root", "Left"));
		string second = HierarchySignature.Compute(BuildTree(a, "Root", "Other"));

		Assert.That(first, Is.Not.EqualTo(second));
	}

	[Test]
	public void DifferentChildCountChangesTheSignature()
	{
		GameBundle bundle = new();
		ProcessedAssetCollection a = bundle.AddNewProcessedCollection("a", Version);

		string first = HierarchySignature.Compute(BuildTree(a, "Root", "Left"));
		string second = HierarchySignature.Compute(BuildTree(a, "Root", "Left", "Right"));

		Assert.That(first, Is.Not.EqualTo(second));
	}

	[Test]
	public void PairingMatchesCorrespondingAssets()
	{
		GameBundle bundle = new();
		ProcessedAssetCollection a = bundle.AddNewProcessedCollection("a", Version);
		ProcessedAssetCollection b = bundle.AddNewProcessedCollection("b", Version);
		IGameObject first = BuildTree(a, "Root", "Left", "Right");
		IGameObject second = BuildTree(b, "Root", "Left", "Right");

		List<(IUnityObjectBase Duplicate, IUnityObjectBase Canonical)> pairs = [];
		bool paired = HierarchySignature.TryPair(second, first, pairs);

		Assert.That(paired, Is.True);
		// 3 GameObjects + 3 Transforms.
		Assert.That(pairs, Has.Count.EqualTo(6));
		Assert.That(pairs[0].Duplicate, Is.SameAs(second));
		Assert.That(pairs[0].Canonical, Is.SameAs(first));
		Assert.That(pairs.All(p => p.Duplicate.ClassID == p.Canonical.ClassID), Is.True);
	}

	[Test]
	public void PairingRefusesMismatchedStructures()
	{
		GameBundle bundle = new();
		ProcessedAssetCollection a = bundle.AddNewProcessedCollection("a", Version);
		IGameObject first = BuildTree(a, "Root", "Left");
		IGameObject second = BuildTree(a, "Root", "Left", "Right");

		List<(IUnityObjectBase Duplicate, IUnityObjectBase Canonical)> pairs = [];

		Assert.That(HierarchySignature.TryPair(second, first, pairs), Is.False);
	}
}
