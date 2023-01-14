using AssetRipper.Export.UnityProjects.Configuration;

namespace AssetRipper.GUI.Components
{
	public class ScriptLanguageVersionConfigDropdown : BaseConfigurationDropdown<ScriptLanguageVersion>
	{
		protected override string GetValueDisplayName(ScriptLanguageVersion value) => value switch
		{
			ScriptLanguageVersion.CSharp1 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_1"],
			ScriptLanguageVersion.CSharp2 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_2"],
			ScriptLanguageVersion.CSharp3 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_3"],
			ScriptLanguageVersion.CSharp4 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_4"],
			ScriptLanguageVersion.CSharp5 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_5"],
			ScriptLanguageVersion.CSharp6 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_6"],
			ScriptLanguageVersion.CSharp7 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_7"],
			ScriptLanguageVersion.CSharp7_1 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_7_1"],
			ScriptLanguageVersion.CSharp7_2 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_7_2"],
			ScriptLanguageVersion.CSharp7_3 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_7_3"],
			ScriptLanguageVersion.CSharp8_0 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_8_0"],
			ScriptLanguageVersion.CSharp9_0 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_9_0"],
			ScriptLanguageVersion.CSharp10_0 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_10_0"],
			ScriptLanguageVersion.CSharp11_0 => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_11_0"],
			ScriptLanguageVersion.Latest => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_latest"],
			ScriptLanguageVersion.AutoSafe => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_auto_safe"],
			ScriptLanguageVersion.AutoExperimental => MainWindow.Instance.LocalizationManager["c_sharp_langage_version_config_auto_experimental"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(ScriptLanguageVersion value)
		{
			return MainWindow.Instance.LocalizationManager["c_sharp_language_version_config_description"];
		}
	}
}
