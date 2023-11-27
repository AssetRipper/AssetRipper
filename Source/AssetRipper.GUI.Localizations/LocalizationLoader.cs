using System.Text.Json;

namespace AssetRipper.GUI.Localizations;

public static partial class LocalizationLoader
{
	private const string LocalizationFilePrefix = "AssetRipper.GUI.Localizations.";

	public static Dictionary<string, string> LoadLanguage(string code)
	{
		return LoadLanguageInternal(AsUnderscoredLanguageCode(code));
	}

	private static Dictionary<string, string> LoadLanguageInternal(string code)
	{
		using Stream stream = typeof(LocalizationLoader).Assembly.GetManifestResourceStream(LocalizationFilePrefix + code + ".json")
			?? throw new NullReferenceException($"Could not load language file {code}.json");

		return JsonSerializer.Deserialize(stream, DictionarySerializerContext.Default.DictionaryStringString)
			?? throw new JsonException($"Could not parse language file {code}.json");
	}
}
