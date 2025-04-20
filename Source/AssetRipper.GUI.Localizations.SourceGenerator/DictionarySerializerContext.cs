using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.GUI.Localizations.SourceGenerator;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal sealed partial class DictionarySerializerContext : JsonSerializerContext
{
	public static Dictionary<string, string> Deserialize(string json)
	{
		return JsonSerializer.Deserialize(json, Default.DictionaryStringString) ?? throw new("Json text could not be deserialized.");
	}
}
