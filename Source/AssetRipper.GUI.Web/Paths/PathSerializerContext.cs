using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Web.Paths;

[JsonSerializable(typeof(AssetPath))]
[JsonSerializable(typeof(BundlePath))]
[JsonSerializable(typeof(CollectionPath))]
[JsonSerializable(typeof(ScenePath))]
[JsonSerializable(typeof(ResourcePath))]
[JsonSerializable(typeof(FailedFilePath))]
internal sealed partial class PathSerializerContext : JsonSerializerContext
{
}
