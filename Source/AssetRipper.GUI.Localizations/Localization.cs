namespace AssetRipper.GUI.Localizations;

public static partial class Localization
{
	private static Dictionary<string, string> CurrentLocale { get; set; }
	private static Dictionary<string, string> FallbackLocale { get; }
	private static string? CurrentLang { get; set; }

	public static event Action OnLanguageChanged = () => { };

	static Localization()
	{
		LoadLanguage("en_US");
		FallbackLocale = CurrentLocale;
	}

	[MemberNotNull(nameof(CurrentLocale), nameof(CurrentLang))]
	public static void LoadLanguage(string code)
	{
		CurrentLang = code;
		CurrentLocale = LocalizationLoader.LoadLanguage(code);
		OnLanguageChanged();
	}

	public static string Get(string key)
	{
		if (CurrentLocale.TryGetValue(key, out string? ret) && !string.IsNullOrEmpty(ret))
		{
			return ret;
		}

		if (FallbackLocale.TryGetValue(key, out ret))
		{
			return ret;
		}

		return $"__{key}__?";
	}
}
