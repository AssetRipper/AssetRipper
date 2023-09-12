using System.Text.Json;

namespace AssetRipper.GUI.Electron;

public readonly record struct ResourcePath(BundlePath BundlePath, int Index)
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
