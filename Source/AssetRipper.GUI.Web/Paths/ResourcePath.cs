using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Web.Paths;

public readonly record struct ResourcePath([property: JsonPropertyName("B")] BundlePath BundlePath, [property: JsonPropertyName("I")] int Index) : IPath<ResourcePath>
{
	public string ToJson()
	{
		return JsonSerializer.Serialize(this, PathSerializerContext.Default.ResourcePath);
	}

	public static ResourcePath FromJson(string json)
	{
		return JsonSerializer.Deserialize(json, PathSerializerContext.Default.ResourcePath);
	}
}
