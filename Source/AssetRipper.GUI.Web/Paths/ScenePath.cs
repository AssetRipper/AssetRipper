using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Web.Paths;

public readonly record struct ScenePath([property: JsonPropertyName("C")] CollectionPath FirstCollection) : IPath<ScenePath>
{
	public static explicit operator ScenePath(CollectionPath first) => new ScenePath(first);

	public string ToJson()
	{
		return JsonSerializer.Serialize(this, PathSerializerContext.Default.ScenePath);
	}

	public static ScenePath FromJson(string json)
	{
		return JsonSerializer.Deserialize(json, PathSerializerContext.Default.ScenePath);
	}
}
