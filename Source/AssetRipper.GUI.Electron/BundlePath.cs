using System.Text.Json;

namespace AssetRipper.GUI.Electron;

public readonly record struct BundlePath(int[]? Path = null)
{
	public static implicit operator ReadOnlySpan<int>(BundlePath path) => path.Path;

	public string ToJson()
	{
		return JsonSerializer.Serialize(this, PathSerializerContext.Default.BundlePath);
	}

	public static BundlePath FromJson(string json)
	{
		return JsonSerializer.Deserialize(json, PathSerializerContext.Default.BundlePath);
	}
}
