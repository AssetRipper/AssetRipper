using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Collections;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Assets.Tests;

internal class AssetEqualityTests
{
	[Test]
	public void DefaultGameObjectEqualityTest()
	{
		IGameObject gameObject1 = AssetCreator.CreateGameObject(new UnityVersion(2017));
		IGameObject gameObject2 = AssetCreator.CreateGameObject(new UnityVersion(2017));
		Assert.That(gameObject1, Is.EqualTo(gameObject2).Using(new AssetEqualityComparer()));
	}

	[Test]
	public void GameObjectWithTransformEqualityTest()
	{
		IGameObject gameObject1 = CreateGameObject();
		IGameObject gameObject2 = CreateGameObject();
		Assert.That(gameObject1, Is.EqualTo(gameObject2).Using(new AssetEqualityComparer()));

		static IGameObject CreateGameObject()
		{
			ProcessedAssetCollection collection = AssetCreator.CreateCollection(new UnityVersion(2017));
			IGameObject gameObject = collection.CreateGameObject();
			ITransform transform = collection.CreateTransform();
			gameObject.AddComponent(ClassIDType.Transform, transform);
			transform.GameObject_C4P = gameObject;
			return gameObject;
		}
	}

	[Test]
	public void GameObjectWithTransformInequalityTest()
	{
		IGameObject gameObject1 = CreateGameObject(0, 0, 0);
		IGameObject gameObject2 = CreateGameObject(1, 1, 1);
		Assert.That(gameObject1, Is.Not.EqualTo(gameObject2).Using(new AssetEqualityComparer()));

		static IGameObject CreateGameObject(float x, float y, float z)
		{
			ProcessedAssetCollection collection = AssetCreator.CreateCollection(new UnityVersion(2017));
			IGameObject gameObject = collection.CreateGameObject();
			ITransform transform = collection.CreateTransform();
			transform.LocalPosition_C4.SetValues(x, y, z);
			gameObject.AddComponent(ClassIDType.Transform, transform);
			transform.GameObject_C4P = gameObject;
			return gameObject;
		}
	}
}
