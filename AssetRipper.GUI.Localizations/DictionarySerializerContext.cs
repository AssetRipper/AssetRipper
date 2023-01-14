using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Localizations;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal sealed partial class DictionarySerializerContext : JsonSerializerContext
{
}
