using System.Text.Json.Serialization;

namespace AssetRipper.Tools.DependenceGrapher;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class DictionarySerializerContext : JsonSerializerContext
{
}
