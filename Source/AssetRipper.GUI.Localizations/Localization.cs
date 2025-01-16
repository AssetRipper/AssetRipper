namespace AssetRipper.GUI.Localizations;

public static partial class Localization
{
	/// <summary>
	/// <see href="https://www.iana.org/assignments/language-subtag-registry/language-subtag-registry" >IANA</see> language code
	/// </summary>
	public static string CurrentLanguageCode { get; private set; } = LanguageCodes.English;

	public static event Action? OnLanguageChanged;

	public static void LoadLanguage(string? code)
	{
		string? value = LanguageCodes.AsHyphenatedLanguageCode(code);
		if (CurrentLanguageCode != value && LanguageCodes.Exists(value))
		{
			CurrentLanguageCode = value;
			OnLanguageChanged?.Invoke();
		}
	}
}
