using System.Text.Json;

namespace AssetRipper.GUI.Electron;

public readonly record struct AssetPath(CollectionPath CollectionPath, long PathID)
{
	public string ToJson()
	{
		return JsonSerializer.Serialize(this,  PathSerializerContext.Default.AssetPath);
	}

	public static AssetPath FromJson(string json)
	{
		return JsonSerializer.Deserialize(json, PathSerializerContext.Default.AssetPath);
	}
}
