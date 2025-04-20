using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.GUI.SourceGenerator;

[JsonSourceGenerationOptions(WriteIndented = true, IndentCharacter = ' ', IndentSize = 4, NewLine = "\n")]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal sealed partial class DictionarySerializerContext : JsonSerializerContext
{
	private static DictionarySerializerContext? _actualDefault;
	private static DictionarySerializerContext ActualDefault
	{
		get
		{
			return _actualDefault ??= new(new JsonSerializerOptions(s_defaultOptions!)
			{
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			});
		}
	}

	public static string Serialize(Dictionary<string, string> dictionary)
	{
		return JsonSerializer.Serialize(dictionary, ActualDefault.DictionaryStringString);
	}

	public static Dictionary<string, string> Deserialize(string json)
	{
		return JsonSerializer.Deserialize(json, ActualDefault.DictionaryStringString) ?? throw new("Json text could not be deserialized.");
	}
}
