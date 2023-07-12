using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Primitives;

namespace AssetRipper.Assets.Tests;

public class SceneDefinitionTests
{
	[Test]
	public void FromName_CreatesSceneDefinitionWithName()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromName("testScene");
		Assert.That(sceneDefinition.Name, Is.EqualTo("testScene"));
	}

	[Test]
	public void FromName_CreatesSceneDefinitionWithGuid()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromName("testScene");
		Assert.That(sceneDefinition.GUID, Is.Not.EqualTo(UnityGuid.Zero));
	}

	[Test]
	public void FromPath_CreatesSceneDefinitionWithPath()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromPath("Assets/Scenes/testScene");
		Assert.That(sceneDefinition.Path, Is.EqualTo("Assets/Scenes/testScene"));
	}

	[Test]
	public void FromPath_CreatesSceneDefinitionWithGuid()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromPath("Assets/Scenes/testScene");
		Assert.That(sceneDefinition.GUID, Is.Not.EqualTo(UnityGuid.Zero));
	}

	[Test]
	public void AddCollection_AddsCollectionToSceneDefinition()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromName("testScene");
		AssetCollection mockAssetCollection = CreateCollection();
		sceneDefinition.AddCollection(mockAssetCollection);
		Assert.That(sceneDefinition.Collections, Has.Count.EqualTo(1));
		Assert.That(sceneDefinition.Collections[0], Is.EqualTo(mockAssetCollection));
	}

	[Test]
	public void AddCollection_ThrowsWhenAssetCollectionIsPartOfDifferentScene()
	{
		SceneDefinition sceneDefinition1 = SceneDefinition.FromName("testScene1");
		AssetCollection mockAssetCollection1 = CreateCollection();
		sceneDefinition1.AddCollection(mockAssetCollection1);

		SceneDefinition sceneDefinition2 = SceneDefinition.FromName("testScene2");
		AssetCollection mockAssetCollection2 = CreateCollection();
		sceneDefinition2.AddCollection(mockAssetCollection2);

		Assert.Throws<InvalidOperationException>(() =>
			sceneDefinition1.AddCollection(mockAssetCollection2));
	}

	[Test]
	public void RemoveCollection_RemovesCollectionFromSceneDefinition()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromName("testScene");
		AssetCollection mockAssetCollection = CreateCollection();
		sceneDefinition.AddCollection(mockAssetCollection);

		sceneDefinition.RemoveCollection(mockAssetCollection);

		Assert.That(sceneDefinition.Collections, Is.Empty);
	}

	[Test]
	public void RemoveCollection_ThrowsWhenCollectionNotPartOfSceneDefinition()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromName("testScene");
		AssetCollection mockAssetCollection1 = CreateCollection();
		AssetCollection mockAssetCollection2 = CreateCollection();

		sceneDefinition.AddCollection(mockAssetCollection1);

		Assert.Throws<ArgumentException>(() =>
			sceneDefinition.RemoveCollection(mockAssetCollection2));

	}

	[Test]
	public void RemoveCollection_DeletesAssetCollectionSceneReference()
	{
		SceneDefinition sceneDefinition = SceneDefinition.FromName("testScene");
		AssetCollection mockAssetCollection = CreateCollection();
		sceneDefinition.AddCollection(mockAssetCollection);

		sceneDefinition.RemoveCollection(mockAssetCollection);

		Assert.That(mockAssetCollection.Scene, Is.Null);
	}

	private static AssetCollection CreateCollection()
	{
		GameBundle gameBundle = new();
		return gameBundle.AddNewProcessedCollection(UnityGuid.NewGuid().ToString(), UnityVersion.MinVersion);
	}
}
