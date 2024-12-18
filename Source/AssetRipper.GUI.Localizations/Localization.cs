namespace AssetRipper.GUI.Localizations;

public static partial class Localization
{
	/// <summary>
	/// <see href="https://www.iana.org/assignments/language-subtag-registry/language-subtag-registry" >IANA</see> language code
	/// </summary>
	public static string CurrentLanguageCode { get; private set; }

	public static event Action? OnLanguageChanged;

	static Localization()
	{
		LoadLanguage("en-US");
	}

	[MemberNotNull(nameof(CurrentLanguageCode))]
	public static void LoadLanguage(string code)
	{
		CurrentLanguageCode = LanguageCodes.AsHyphenatedLanguageCode(code);
		OnLanguageChanged?.Invoke();
	}
}
