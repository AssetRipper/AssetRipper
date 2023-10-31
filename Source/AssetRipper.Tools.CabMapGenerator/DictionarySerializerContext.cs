using System.Text.Json.Serialization;

namespace AssetRipper.Tools.CabMapGenerator;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class DictionarySerializerContext : JsonSerializerContext
{
}
