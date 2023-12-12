using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.Export.PrimaryContent;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using System.Text.Json.Nodes;

namespace AssetRipper.Tests;

internal class DefaultJsonWalkerTests
{
	[Test]
	public static void SerializedCustomObjectIsValidJson()
	{
		string json = new DefaultJsonWalker().SerializeStandard(CustomInjectedObject.Create());
		JsonNode? node = JsonNode.Parse(json);
		Assert.That(node, Is.Not.Null);
		Assert.That(node, Is.TypeOf<JsonObject>());
	}

	private sealed class CustomInjectedObject : InjectedUnityObject<CustomInjectedObject>
	{
		private readonly bool boolean = true;
		private readonly string name = nameof(CustomInjectedObject);
		private readonly int integer = 42;
		private readonly IMesh? mesh;

		public CustomInjectedObject(AssetInfo assetInfo) : base(assetInfo)
		{
		}

		public static CustomInjectedObject Create()
		{
			return AssetCreator.CreateAsset(default, default, (assetInfo) => new CustomInjectedObject(assetInfo));
		}

		public override void WalkStandard(AssetWalker walker)
		{
			if (walker.EnterAsset(this))
			{
				WalkPrimitiveField(walker, boolean);
				walker.DivideAsset(this);
				WalkPrimitiveField(walker, name);
				walker.DivideAsset(this);
				WalkPrimitiveField(walker, integer);
				walker.DivideAsset(this);
				WalkPPtrField(walker, mesh);
				walker.ExitAsset(this);
			}
		}
	}
}
