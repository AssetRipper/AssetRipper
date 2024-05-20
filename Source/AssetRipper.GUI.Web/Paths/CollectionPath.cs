using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Web.Paths;

public readonly record struct CollectionPath([property: JsonPropertyName("B")] BundlePath BundlePath, [property: JsonPropertyName("I")] int Index) : IPath<CollectionPath>
{
	public AssetPath GetAsset(long pathID)
	{
		return new(this, pathID);
	}

	public string ToJson()
	{
		return JsonSerializer.Serialize(this, PathSerializerContext.Default.CollectionPath);
	}

	public static CollectionPath FromJson(string json)
	{
		return JsonSerializer.Deserialize(json, PathSerializerContext.Default.CollectionPath);
	}
}
