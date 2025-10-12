using AssetRipper.Assets.Cloning;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Assets.Tests;

internal class TransformCopyValuesTests
{
	private ITransform transform;

	[SetUp]
	public void Setup()
	{
		transform = AssetCreator.CreateTransform(new UnityVersion(2017));
	}

	[Test]
	public void CopyingSelfDoesNotClearPosition()
	{
		transform.LocalPosition_C4.SetValues(1, 2, 3);
		transform.CopyValues(transform);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(transform.LocalPosition_C4.X, Is.EqualTo(1));
			Assert.That(transform.LocalPosition_C4.Y, Is.EqualTo(2));
			Assert.That(transform.LocalPosition_C4.Z, Is.EqualTo(3));
		}
	}

	[Test]
	public void CopyingSelfDoesNotClearGameObject()
	{
		IGameObject gameObject = AssetCreator.CreateGameObject(new UnityVersion(2017));
		transform.GameObject_C4P = gameObject;
		transform.CopyValues(transform);
		Assert.That(transform.GameObject_C4P, Is.SameAs(gameObject));
	}

	[Test]
	public void ReplacingChildSucceeds()
	{
		ITransform child = AssetCreator.CreateTransform(new UnityVersion(2017));
		transform.Children_C4P.Add(child);
		ITransform newChild = AssetCreator.CreateTransform(new UnityVersion(2017));
		SingleReplacementAssetResolver resolver = new(child, newChild);
		transform.CopyValues(transform, new PPtrConverter(transform.Collection, transform.Collection, resolver));
		Assert.That(transform.Children_C4P, Has.Count.EqualTo(1));
		Assert.That(transform.Children_C4P[0], Is.SameAs(newChild));
	}
}
