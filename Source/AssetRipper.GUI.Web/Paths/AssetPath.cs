using System.Text.Json;

namespace AssetRipper.GUI.Web.Paths;

public readonly record struct AssetPath(CollectionPath CollectionPath, long PathID) : IPath<AssetPath>
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
