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
	public async Task DefaultGameObjectEqualityTest()
	{
		IGameObject gameObject1 = AssetCreator.CreateGameObject(new UnityVersion(2017));
		IGameObject gameObject2 = AssetCreator.CreateGameObject(new UnityVersion(2017));
		await Assert.That(new AssetEqualityComparer().Equals(gameObject1, gameObject2)).IsTrue();
	}

	[Test]
	public async Task GameObjectWithTransformEqualityTest()
	{
		IGameObject gameObject1 = CreateGameObject();
		IGameObject gameObject2 = CreateGameObject();
		await Assert.That(new AssetEqualityComparer().Equals(gameObject1, gameObject2)).IsTrue();

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
	public async Task GameObjectWithTransformInequalityTest()
	{
		IGameObject gameObject1 = CreateGameObject(0, 0, 0);
		IGameObject gameObject2 = CreateGameObject(1, 1, 1);
		await Assert.That(new AssetEqualityComparer().Equals(gameObject1, gameObject2)).IsFalse();

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