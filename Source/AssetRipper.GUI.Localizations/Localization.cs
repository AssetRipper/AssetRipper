namespace AssetRipper.GUI.Localizations;

public static partial class Localization
{
	private static Dictionary<string, string> CurrentDictionary { get; set; }
	/// <summary>
	/// The (English) dictionary to use if <see cref="CurrentDictionary"/> doesn't have a key.
	/// </summary>
	private static Dictionary<string, string> FallbackDictionary { get; }
	/// <summary>
	/// <see href="https://www.iana.org/assignments/language-subtag-registry/language-subtag-registry" >IANA</see> language code
	/// </summary>
	public static string CurrentLanguageCode { get; private set; }

	public static event Action OnLanguageChanged = () => { };

	static Localization()
	{
		LoadLanguage("en-US");
		FallbackDictionary = CurrentDictionary;
	}

	[MemberNotNull(nameof(CurrentDictionary), nameof(CurrentLanguageCode))]
	public static void LoadLanguage(string code)
	{
		CurrentLanguageCode = LocalizationLoader.AsHyphenatedLanguageCode(code);
		CurrentDictionary = LocalizationLoader.LoadLanguage(CurrentLanguageCode);
		OnLanguageChanged();
	}

	private static string Get(string key)
	{
		if (CurrentDictionary.TryGetValue(key, out string? ret) && !string.IsNullOrEmpty(ret))
		{
			return ret;
		}

		if (FallbackDictionary.TryGetValue(key, out ret))
		{
			return ret;
		}

		//This should never happen unless I edit the json without running the source generator.
		return $"__{key}__?";
	}
}
