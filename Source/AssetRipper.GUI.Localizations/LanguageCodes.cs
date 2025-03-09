namespace AssetRipper.GUI.Localizations;

public static partial class LanguageCodes
{
	public const string English = "en-US";

	public static bool Exists([NotNullWhen(true)] string? code)
	{
		return code is not null && LanguageNameDictionary.ContainsKey(code);
	}
}
