using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Electron;

[JsonSerializable(typeof(AssetPath))]
internal sealed partial class PathSerializerContext : JsonSerializerContext
{
}
