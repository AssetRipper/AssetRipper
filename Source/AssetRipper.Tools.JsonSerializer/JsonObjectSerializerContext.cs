using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AssetRipper.Tools.JsonSerializer;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(JsonObject))]
internal partial class JsonObjectSerializerContext : JsonSerializerContext
{
}
