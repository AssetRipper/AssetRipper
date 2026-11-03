using AssetRipper.Assets.Cloning;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Assets.Tests;

internal class TransformCopyValuesTests
{
	private readonly ITransform transform = AssetCreator.CreateTransform(new UnityVersion(2017));

	[Test]
	public async Task CopyingSelfDoesNotClearPosition()
	{
		transform.LocalPosition_C4.SetValues(1, 2, 3);
		transform.CopyValues(transform);

		{
			await Assert.That(transform.LocalPosition_C4.X).IsEqualTo(1);
			await Assert.That(transform.LocalPosition_C4.Y).IsEqualTo(2);
			await Assert.That(transform.LocalPosition_C4.Z).IsEqualTo(3);
		}
	}

	[Test]
	public async Task CopyingSelfDoesNotClearGameObject()
	{
		IGameObject gameObject = AssetCreator.CreateGameObject(new UnityVersion(2017));
		transform.GameObject_C4P = gameObject;
		transform.CopyValues(transform);
		await Assert.That(transform.GameObject_C4P).IsSameReferenceAs(gameObject);
	}

	[Test]
	public async Task ReplacingChildSucceeds()
	{
		ITransform child = AssetCreator.CreateTransform(new UnityVersion(2017));
		transform.Children_C4P.Add(child);
		ITransform newChild = AssetCreator.CreateTransform(new UnityVersion(2017));
		SingleReplacementAssetResolver resolver = new(child, newChild);
		transform.CopyValues(transform, new PPtrConverter(transform.Collection, transform.Collection, resolver));
		await Assert.That(transform.Children_C4P).Count().IsEqualTo(1);
		await Assert.That(transform.Children_C4P[0]).IsSameReferenceAs(newChild);
	}
}
