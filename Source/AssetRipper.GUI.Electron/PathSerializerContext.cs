using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Electron;

[JsonSerializable(typeof(AssetPath))]
[JsonSerializable(typeof(BundlePath))]
[JsonSerializable(typeof(CollectionPath))]
[JsonSerializable(typeof(ResourcePath))]
internal sealed partial class PathSerializerContext : JsonSerializerContext
{
}
