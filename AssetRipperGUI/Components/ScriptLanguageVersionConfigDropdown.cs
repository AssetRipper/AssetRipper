using ICSharpCode.Decompiler.CSharp;

namespace AssetRipper.GUI.Components
{
	public class ScriptLanguageVersionConfigDropdown : BaseConfigurationDropdown<LanguageVersion>
	{
		protected override string GetValueDisplayName(LanguageVersion value) => value switch
		{
			LanguageVersion.CSharp1 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_1"],
			LanguageVersion.CSharp2 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_2"],
			LanguageVersion.CSharp3 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_3"],
			LanguageVersion.CSharp4 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_4"],
			LanguageVersion.CSharp5 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_5"],
			LanguageVersion.CSharp6 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_6"],
			LanguageVersion.CSharp7 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_7"],
			LanguageVersion.CSharp7_1 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_7_1"],
			LanguageVersion.CSharp7_2 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_7_2"],
			LanguageVersion.CSharp7_3 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_7_3"],
			LanguageVersion.CSharp8_0 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_8_0"],
			LanguageVersion.Latest => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_latest"],
			LanguageVersion.Preview => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_preview"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(LanguageVersion value)
		{
			return MainWindow.Instance.LocalizationManager["c_sharp_language_version_config_description"];
		}
	}
}
