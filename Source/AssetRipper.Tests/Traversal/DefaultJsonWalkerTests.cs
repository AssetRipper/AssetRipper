using AssetRipper.Assets;
using AssetRipper.Export.PrimaryContent;
using AssetRipper.SourceGenerated.Extensions;
using System.Globalization;
using System.Text.Json.Nodes;

namespace AssetRipper.Tests.Traversal;

internal class DefaultJsonWalkerTests
{
	[Test]
	[MethodDataSource(nameof(GetObjectTypes))]
	public async Task SerializedCustomObjectIsValidJson(Type type)
	{
		UnityObjectBase asset = AssetCreator.CreateUnsafe(type);
		StringWriter stringWriter = new(CultureInfo.InvariantCulture) { NewLine = "\n" };
		asset.WalkStandard(new DefaultJsonWalker(stringWriter));
		string json = stringWriter.ToString();
		JsonNode? node = JsonNode.Parse(json);
		await Assert.That(node).IsNotNull();
		await Assert.That(node).IsTypeOf<JsonObject>();
	}

	public static Type[] GetObjectTypes() =>
	[
		typeof(SimpleObject),
		typeof(ParentObject),
		typeof(PrimitiveListObject),
		typeof(ComponentListObject),
		typeof(ListObject),
		typeof(DictionaryObject),
		typeof(PairObject),
	];
}