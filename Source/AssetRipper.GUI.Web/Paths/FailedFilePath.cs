using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Web.Paths;

public readonly record struct FailedFilePath([property: JsonPropertyName("B")] BundlePath BundlePath, [property: JsonPropertyName("I")] int Index) : IPath<FailedFilePath>
{
	public string ToJson()
	{
		return JsonSerializer.Serialize(this, PathSerializerContext.Default.FailedFilePath);
	}

	public static FailedFilePath FromJson(string json)
	{
		return JsonSerializer.Deserialize(json, PathSerializerContext.Default.FailedFilePath);
	}
}
