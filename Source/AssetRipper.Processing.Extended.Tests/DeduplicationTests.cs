using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Primitives;
using AssetRipper.Processing.Extended.Deduplication;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing.Extended.Tests;

public class DeduplicationTests
{
	private static readonly UnityVersion Version = new(2020, 1);

	private static ITextAsset CreateTextAsset(ProcessedAssetCollection collection, string name, string script)
	{
		ITextAsset asset = collection.CreateTextAsset();
		asset.Name = name;
		asset.Script_C49 = script;
		return asset;
	}

	[Test]
	public void IdenticalContentHashesEqual()
	{
		GameBundle bundle = new();
		ProcessedAssetCollection a = bundle.AddNewProcessedCollection("a", Version);
		ProcessedAssetCollection b = bundle.AddNewProcessedCollection("b", Version);

		string hashA = AssetContentHasher.Compute(CreateTextAsset(a, "Same", "content"));
		string hashB = AssetContentHasher.Compute(CreateTextAsset(b, "Same", "content"));

		Assert.That(hashA, Is.EqualTo(hashB));
	}

	[Test]
	public void DifferentContentHashesDiffer()
	{
		GameBundle bundle = new();
		ProcessedAssetCollection a = bundle.AddNewProcessedCollection("a", Version);

		string hashA = AssetContentHasher.Compute(CreateTextAsset(a, "Same", "one"));
		string hashB = AssetContentHasher.Compute(CreateTextAsset(a, "Same", "two"));

		Assert.That(hashA, Is.Not.EqualTo(hashB));
	}

	[Test]
	public void RedirectsDuplicateAcrossCollections()
	{
		GameBundle bundle = new();
		ProcessedAssetCollection a = bundle.AddNewProcessedCollection("aaa", Version);
		ProcessedAssetCollection b = bundle.AddNewProcessedCollection("bbb", Version);
		ITextAsset first = CreateTextAsset(a, "Shared", "payload");
		ITextAsset second = CreateTextAsset(b, "Shared", "payload");

		DeduplicationMap map = new();
		new AssetDeduplicationProcessor(map).Process(new GameData(bundle, Version, null!, null));

		Assert.That(map.Count, Is.EqualTo(1));
		// "aaa" sorts before "bbb", so the first collection supplies the canonical asset.
		Assert.That(map.TryGetCanonical(second, out IUnityObjectBase? canonical), Is.True);
		Assert.That(canonical, Is.SameAs(first));
		Assert.That(map.TryGetCanonical(first, out _), Is.False);
	}

	[Test]
	public void LeavesDuplicatesWithinOneCollectionAlone()
	{
		GameBundle bundle = new();
		ProcessedAssetCollection a = bundle.AddNewProcessedCollection("a", Version);
		CreateTextAsset(a, "Shared", "payload");
		CreateTextAsset(a, "Shared", "payload");

		DeduplicationMap map = new();
		new AssetDeduplicationProcessor(map).Process(new GameData(bundle, Version, null!, null));

		Assert.That(map.Count, Is.Zero);
	}

	[Test]
	public void LeavesDistinctAssetsAlone()
	{
		GameBundle bundle = new();
		ProcessedAssetCollection a = bundle.AddNewProcessedCollection("a", Version);
		ProcessedAssetCollection b = bundle.AddNewProcessedCollection("b", Version);
		CreateTextAsset(a, "One", "alpha");
		CreateTextAsset(b, "Two", "beta");

		DeduplicationMap map = new();
		new AssetDeduplicationProcessor(map).Process(new GameData(bundle, Version, null!, null));

		Assert.That(map.Count, Is.Zero);
	}

	[Test]
	public void ElectionIsDeterministicAcrossRuns()
	{
		static (DeduplicationMap Map, ITextAsset Second) Run()
		{
			GameBundle bundle = new();
			ProcessedAssetCollection a = bundle.AddNewProcessedCollection("aaa", Version);
			ProcessedAssetCollection b = bundle.AddNewProcessedCollection("bbb", Version);
			CreateTextAsset(a, "Shared", "payload");
			ITextAsset second = CreateTextAsset(b, "Shared", "payload");
			DeduplicationMap map = new();
			new AssetDeduplicationProcessor(map).Process(new GameData(bundle, Version, null!, null));
			return (map, second);
		}

		(DeduplicationMap first, ITextAsset firstSecond) = Run();
		(DeduplicationMap again, ITextAsset againSecond) = Run();

		Assert.Multiple(() =>
		{
			Assert.That(first.TryGetCanonical(firstSecond, out IUnityObjectBase? c1), Is.True);
			Assert.That(again.TryGetCanonical(againSecond, out IUnityObjectBase? c2), Is.True);
			Assert.That(c1!.Collection.Name, Is.EqualTo(c2!.Collection.Name));
		});
	}

	[Test]
	public void MapRejectsSelfReplacement()
	{
		GameBundle bundle = new();
		ProcessedAssetCollection a = bundle.AddNewProcessedCollection("a", Version);
		ITextAsset asset = CreateTextAsset(a, "X", "y");

		Assert.Throws<ArgumentException>(() => new DeduplicationMap().Add(asset, asset));
	}
}
