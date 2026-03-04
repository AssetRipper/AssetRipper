using AssetRipper.Assets;
using AssetRipper.Export.PrimaryContent;
using AssetRipper.SourceGenerated.Extensions;
using System.Globalization;
using System.Text.Json.Nodes;

namespace AssetRipper.Tests.Traversal;

internal class DefaultJsonWalkerTests
{
	[TestCaseSource(nameof(GetObjectTypes))]
	public static void SerializedCustomObjectIsValidJson(Type type)
	{
		UnityObjectBase asset = AssetCreator.CreateUnsafe(type);
		StringWriter stringWriter = new(CultureInfo.InvariantCulture) { NewLine = "\n" };
		asset.WalkStandard(new DefaultJsonWalker(stringWriter));
		string json = stringWriter.ToString();
		JsonNode? node = JsonNode.Parse(json);
		Assert.That(node, Is.Not.Null);
		Assert.That(node, Is.TypeOf<JsonObject>());
	}

	private static Type[] GetObjectTypes() =>
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
