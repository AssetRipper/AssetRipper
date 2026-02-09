using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Web.Paths;

public readonly record struct AssetPath([property: JsonPropertyName("C")] CollectionPath CollectionPath, [property: JsonPropertyName("D")] long PathID) : IPath<AssetPath>
{
	public string ToJson()
	{
		return JsonSerializer.Serialize(this, PathSerializerContext.Default.AssetPath);
	}

	public static AssetPath FromJson(string json)
	{
		return JsonSerializer.Deserialize(json, PathSerializerContext.Default.AssetPath);
	}
}
