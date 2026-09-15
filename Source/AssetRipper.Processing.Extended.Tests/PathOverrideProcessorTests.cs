using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Primitives;
using AssetRipper.Processing.Extended.PathOverrides;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing.Extended.Tests;

public class PathOverrideProcessorTests
{
	private static readonly UnityVersion Version = new(2020, 1);

	private static (GameData GameData, ProcessedAssetCollection Collection) CreateGameData(string collectionName)
	{
		GameBundle bundle = new();
		ProcessedAssetCollection collection = bundle.AddNewProcessedCollection(collectionName, Version);
		GameData gameData = new(bundle, Version, null!, null);
		return (gameData, collection);
	}

	[Test]
	public void AppliesOverrideToMatchingAsset()
	{
		(GameData gameData, ProcessedAssetCollection collection) = CreateGameData("level1.assets");
		IGameObject gameObject = collection.CreateGameObject();

		PathOverrideData data = new();
		data.Files.Add("level1.assets", new Dictionary<long, string> { { gameObject.PathID, "Assets/Prefabs/Renamed.prefab" } });

		new PathOverrideProcessor(data).Process(gameData);

		Assert.That(gameObject.OverridePath, Is.EqualTo("Assets/Prefabs/Renamed.prefab"));
	}

	[Test]
	public void LeavesUnlistedAssetsUntouched()
	{
		(GameData gameData, ProcessedAssetCollection collection) = CreateGameData("level1.assets");
		IGameObject listed = collection.CreateGameObject();
		IGameObject unlisted = collection.CreateGameObject();

		PathOverrideData data = new();
		data.Files.Add("level1.assets", new Dictionary<long, string> { { listed.PathID, "Assets/Listed.prefab" } });

		new PathOverrideProcessor(data).Process(gameData);

		Assert.That(unlisted.OverridePath, Is.Null);
	}

	[Test]
	public void UnknownCollectionNameDoesNotThrow()
	{
		(GameData gameData, _) = CreateGameData("level1.assets");

		PathOverrideData data = new();
		data.Files.Add("does-not-exist.assets", new Dictionary<long, string> { { 1L, "Assets/Nope.asset" } });

		Assert.DoesNotThrow(() => new PathOverrideProcessor(data).Process(gameData));
	}

	[Test]
	public void UnknownPathIdDoesNotThrow()
	{
		(GameData gameData, ProcessedAssetCollection collection) = CreateGameData("level1.assets");
		collection.CreateGameObject();

		PathOverrideData data = new();
		data.Files.Add("level1.assets", new Dictionary<long, string> { { 999999L, "Assets/Nope.asset" } });

		Assert.DoesNotThrow(() => new PathOverrideProcessor(data).Process(gameData));
	}

	[Test]
	public void EmptyDataIsANoOp()
	{
		(GameData gameData, ProcessedAssetCollection collection) = CreateGameData("level1.assets");
		IGameObject gameObject = collection.CreateGameObject();

		new PathOverrideProcessor(new PathOverrideData()).Process(gameData);

		Assert.That(gameObject.OverridePath, Is.Null);
	}
}
