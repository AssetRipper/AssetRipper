using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Assets.Tests;

internal class GameObjectCopyValuesTests
{
	private IGameObject gameObject;

	[SetUp]
	public void Setup()
	{
		gameObject = AssetCreator.CreateGameObject(new UnityVersion(2017));
	}

	[Test]
	public void CopyingSelfDoesNotClearPrimitives()
	{
		gameObject.Layer = 1;
		gameObject.CopyValues(gameObject);
		Assert.That(gameObject.Layer, Is.EqualTo(1));
	}

	[Test]
	public void CopyingSelfDoesNotClearLists()
	{
		IGameObject gameObject = AssetCreator.CreateGameObject(new UnityVersion(2017));
		gameObject.Components.AddNew();
		Assert.That(gameObject.Components, Has.Count.EqualTo(1));
		gameObject.CopyValues(gameObject);
		Assert.That(gameObject.Components, Has.Count.EqualTo(1));
	}
}
