using AssetRipper.GUI.Localizations;
using AssetRipper.Import.Logging;

namespace AssetRipper.GUI.Electron;

public static class Localization
{
	private static Dictionary<string, string> CurrentLocale;
	private static readonly Dictionary<string, string> FallbackLocale;
	private static string? CurrentLang;

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
		Logger.Info(LogCategory.System, $"Loading locale {code}.json");
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
			Logger.Verbose(LogCategory.System, $"Locale {CurrentLang} is missing a definition for {key}. Using fallback language (en_US)");
			return ret;
		}

		Logger.Warning(LogCategory.System, $"Locale {CurrentLang} is missing a definition for {key}, and it also could not be found in the fallback language (en_US)");
		return $"__{key}__?";
	}
}
