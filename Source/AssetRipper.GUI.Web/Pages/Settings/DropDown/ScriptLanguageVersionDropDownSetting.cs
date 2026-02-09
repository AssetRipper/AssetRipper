using AssetRipper.Export.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings.DropDown;

public sealed class ScriptLanguageVersionDropDownSetting : DropDownSetting<ScriptLanguageVersion>
{
	public static ScriptLanguageVersionDropDownSetting Instance { get; } = new();

	public override string Title => Localization.ScriptLanguageVersionTitle;

	protected override string GetDisplayName(ScriptLanguageVersion value) => value switch
	{
		ScriptLanguageVersion.AutoExperimental => Localization.CSharpLangageVersionConfigAutoExperimental,
		ScriptLanguageVersion.AutoSafe => Localization.CSharpLangageVersionConfigAutoSafe,
		ScriptLanguageVersion.CSharp1 => Localization.CSharpLangageVersionConfig1,
		ScriptLanguageVersion.CSharp2 => Localization.CSharpLangageVersionConfig2,
		ScriptLanguageVersion.CSharp3 => Localization.CSharpLangageVersionConfig3,
		ScriptLanguageVersion.CSharp4 => Localization.CSharpLangageVersionConfig4,
		ScriptLanguageVersion.CSharp5 => Localization.CSharpLangageVersionConfig5,
		ScriptLanguageVersion.CSharp6 => Localization.CSharpLangageVersionConfig6,
		ScriptLanguageVersion.CSharp7 => Localization.CSharpLangageVersionConfig7,
		ScriptLanguageVersion.CSharp7_1 => Localization.CSharpLangageVersionConfig71,
		ScriptLanguageVersion.CSharp7_2 => Localization.CSharpLangageVersionConfig72,
		ScriptLanguageVersion.CSharp7_3 => Localization.CSharpLangageVersionConfig73,
		ScriptLanguageVersion.CSharp8_0 => Localization.CSharpLangageVersionConfig80,
		ScriptLanguageVersion.CSharp9_0 => Localization.CSharpLangageVersionConfig90,
		ScriptLanguageVersion.CSharp10_0 => Localization.CSharpLangageVersionConfig100,
		ScriptLanguageVersion.CSharp11_0 => Localization.CSharpLangageVersionConfig110,
		ScriptLanguageVersion.CSharp12_0 => Localization.CSharpLangageVersionConfig120,
		ScriptLanguageVersion.Latest => Localization.CSharpLangageVersionConfigLatest,
		_ => base.GetDisplayName(value),
	};

	protected override string? GetDescription(ScriptLanguageVersion value) => Localization.CSharpLanguageVersionConfigDescription;
}
