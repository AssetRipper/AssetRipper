using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Assets.Tests;

internal class GameObjectCopyValuesTests
{
	[Test]
	public async Task CopyingSelfDoesNotClearPrimitives()
	{
		IGameObject gameObject = AssetCreator.CreateGameObject(new UnityVersion(2017));
		gameObject.Layer = 1;
		gameObject.CopyValues(gameObject);
		await Assert.That(gameObject.Layer).IsEqualTo(1u);
	}

	[Test]
	public async Task CopyingSelfDoesNotClearLists()
	{
		IGameObject gameObject = AssetCreator.CreateGameObject(new UnityVersion(2017));
		gameObject.Components.AddNew();
		await Assert.That(gameObject.Components).Count().IsEqualTo(1);
		gameObject.CopyValues(gameObject);
		await Assert.That(gameObject.Components).Count().IsEqualTo(1);
	}
}
