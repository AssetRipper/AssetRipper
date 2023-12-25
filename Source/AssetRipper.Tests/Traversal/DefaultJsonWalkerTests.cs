using AssetRipper.Assets;
using AssetRipper.Export.PrimaryContent;
using System.Text.Json.Nodes;

namespace AssetRipper.Tests.Traversal;

internal class DefaultJsonWalkerTests
{
	[TestCaseSource(nameof(GetObjectTypes))]
	public static void SerializedCustomObjectIsValidJson(Type type)
	{
		UnityObjectBase asset = AssetCreator.CreateUnsafe(type);
		string json = new DefaultJsonWalker().SerializeStandard(asset);
		JsonNode? node = JsonNode.Parse(json);
		Assert.That(node, Is.Not.Null);
		Assert.That(node, Is.TypeOf<JsonObject>());
	}

	private static IEnumerable<Type> GetObjectTypes()
	{
		yield return typeof(SimpleObject);
		yield return typeof(ParentObject);
		yield return typeof(PrimitiveListObject);
		yield return typeof(ComponentListObject);
		yield return typeof(ListObject);
		yield return typeof(DictionaryObject);
		yield return typeof(PairObject);
	}
}
