using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Primitives;

namespace AssetRipper.Assets.Tests;

public class SceneDefinitionTests
{
	[Test]
	public async Task FromName_CreatesSceneDefinitionWithName()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromName("testScene");
		await Assert.That(sceneDefinition.Name).IsEqualTo("testScene");
	}

	[Test]
	public async Task FromName_CreatesSceneDefinitionWithGuid()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromName("testScene");
		await Assert.That(sceneDefinition.GUID).IsNotEqualTo(UnityGuid.Zero);
	}

	[Test]
	public async Task FromPath_CreatesSceneDefinitionWithPath()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromPath("Assets/Scenes/testScene");
		await Assert.That(sceneDefinition.Path).IsEqualTo("Assets/Scenes/testScene");
	}

	[Test]
	public async Task FromPath_CreatesSceneDefinitionWithGuid()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromPath("Assets/Scenes/testScene");
		await Assert.That(sceneDefinition.GUID).IsNotEqualTo(UnityGuid.Zero);
	}

	[Test]
	public async Task AddCollection_AddsCollectionToSceneDefinition()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromName("testScene");
		AssetCollection mockAssetCollection = CreateCollection();
		sceneDefinition.AddCollection(mockAssetCollection);
		await Assert.That(sceneDefinition.Collections).Count().IsEqualTo(1);
		await Assert.That(sceneDefinition.Collections[0]).IsEqualTo(mockAssetCollection);
	}

	[Test]
	public async Task AddCollection_ThrowsWhenAssetCollectionIsPartOfDifferentScene()
	{
		SceneDefinition sceneDefinition1 = SceneDefinition.FromName("testScene1");
		AssetCollection mockAssetCollection1 = CreateCollection();
		sceneDefinition1.AddCollection(mockAssetCollection1);

		SceneDefinition sceneDefinition2 = SceneDefinition.FromName("testScene2");
		AssetCollection mockAssetCollection2 = CreateCollection();
		sceneDefinition2.AddCollection(mockAssetCollection2);

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			Task.Run(() => sceneDefinition1.AddCollection(mockAssetCollection2)));
	}

	[Test]
	public async Task RemoveCollection_RemovesCollectionFromSceneDefinition()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromName("testScene");
		AssetCollection mockAssetCollection = CreateCollection();
		sceneDefinition.AddCollection(mockAssetCollection);

		sceneDefinition.RemoveCollection(mockAssetCollection);

		await Assert.That(sceneDefinition.Collections).IsEmpty();
	}

	[Test]
	public async Task RemoveCollection_ThrowsWhenCollectionNotPartOfSceneDefinition()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromName("testScene");
		AssetCollection mockAssetCollection1 = CreateCollection();
		AssetCollection mockAssetCollection2 = CreateCollection();

		sceneDefinition.AddCollection(mockAssetCollection1);

		await Assert.ThrowsAsync<ArgumentException>(() =>
			Task.Run(() => sceneDefinition.RemoveCollection(mockAssetCollection2)));

	}

	[Test]
	public async Task RemoveCollection_DeletesAssetCollectionSceneReference()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromName("testScene");
		AssetCollection mockAssetCollection = CreateCollection();
		sceneDefinition.AddCollection(mockAssetCollection);

		sceneDefinition.RemoveCollection(mockAssetCollection);

		await Assert.That(mockAssetCollection.Scene).IsNull();
	}

	private static AssetCollection CreateCollection()
	{
		GameBundle gameBundle = new();
		return gameBundle.AddNewProcessedCollection(UnityGuid.NewGuid().ToString(), UnityVersion.MinVersion);
	}
}