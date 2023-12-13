using System.Text.Json;

namespace AssetRipper.GUI.Web.Paths;

public readonly record struct ResourcePath(BundlePath BundlePath, int Index) : IPath<ResourcePath>
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
