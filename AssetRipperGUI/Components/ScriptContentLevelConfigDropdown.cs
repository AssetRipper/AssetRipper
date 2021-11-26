using AssetRipper.Core.Configuration;

namespace AssetRipper.GUI.Components
{
	public class ScriptContentLevelConfigDropdown : BaseConfigurationDropdown<ScriptContentLevel>
	{
		protected override string GetValueDisplayName(ScriptContentLevel value) => value switch
		{
			ScriptContentLevel.Level0 => MainWindow.Instance.LocalizationManager["script_content_level_0"],
			ScriptContentLevel.Level1 => MainWindow.Instance.LocalizationManager["script_content_level_1"],
			ScriptContentLevel.Level2 => MainWindow.Instance.LocalizationManager["script_content_level_2"],
			ScriptContentLevel.Level3 => MainWindow.Instance.LocalizationManager["script_content_level_3"],
			ScriptContentLevel.Level4 => MainWindow.Instance.LocalizationManager["script_content_level_4"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(ScriptContentLevel value) => value switch
		{
			ScriptContentLevel.Level0 => MainWindow.Instance.LocalizationManager["script_content_level_0_description"],
			ScriptContentLevel.Level1 => MainWindow.Instance.LocalizationManager["script_content_level_1_description"],
			ScriptContentLevel.Level2 => MainWindow.Instance.LocalizationManager["script_content_level_2_description"],
			ScriptContentLevel.Level3 => MainWindow.Instance.LocalizationManager["script_content_level_3_description"],
			ScriptContentLevel.Level4 => MainWindow.Instance.LocalizationManager["script_content_level_4_description"],
			_ => null,
		};
	}
}
