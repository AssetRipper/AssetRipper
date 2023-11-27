// Auto-generated code. Do not modify manually.

namespace AssetRipper.GUI.Localizations;
partial class LocalizationLoader
{
	/// <summary>
	/// Dictionary of Language codes to Language names
	/// </summary>
	public static IReadOnlyDictionary<string, string> LanguageNameDictionary { get; } = new Dictionary<string, string>
	{
		{ "ar", "العربية" },
		{ "de", "Deutsch" },
		{ "en-US", "English (United States)" },
		{ "es", "español" },
		{ "fr", "français" },
		{ "id", "Indonesia" },
		{ "it", "italiano" },
		{ "ja", "日本語" },
		{ "ko", "한국어" },
		{ "nl", "Nederlands" },
		{ "pl", "polski" },
		{ "pt-BR", "português (Brasil)" },
		{ "ro", "română" },
		{ "ru", "русский" },
		{ "tr", "Türkçe" },
		{ "uk", "українська" },
		{ "zh-Hans", "中文（简体）" },
		{ "zh-Hant", "中文（繁體）" },
	};

	/// <summary>
	/// Convert a language code to its underscored variant.
	/// </summary>
	/// <remarks>
	/// This is used by the json file names.
	/// </remarks>
	internal static string AsUnderscoredLanguageCode(string value) => value switch
	{
		"en-US" => "en_US",
		"pt-BR" => "pt_BR",
		"zh-Hans" => "zh_Hans",
		"zh-Hant" => "zh_Hant",
		_ => value
	};

	/// <summary>
	/// Convert a language code to its hyphenated variant.
	/// </summary>
	/// <remarks>
	/// This is used by HTML and the IANA Language Subtag Registry.
	/// </remarks>
	internal static string AsHyphenatedLanguageCode(string value) => value switch
	{
		"en_US" => "en-US",
		"pt_BR" => "pt-BR",
		"zh_Hans" => "zh-Hans",
		"zh_Hant" => "zh-Hant",
		_ => value
	};
}
