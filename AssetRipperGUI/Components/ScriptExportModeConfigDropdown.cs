using AssetRipper.Library.Configuration;

namespace AssetRipper.GUI.Components
{
	public class ScriptExportModeConfigDropdown : BaseConfigurationDropdown<ScriptExportMode>
	{
		protected override string GetValueDisplayName(ScriptExportMode value) => value switch
		{
			ScriptExportMode.Decompiled => MainWindow.Instance.LocalizationManager["script_export_format_decompiled"],
			ScriptExportMode.Hybrid => MainWindow.Instance.LocalizationManager["script_export_format_hybrid"],
			ScriptExportMode.DllExportWithRenaming => MainWindow.Instance.LocalizationManager["script_export_format_dll_with_renaming"],
			ScriptExportMode.DllExportWithoutRenaming => MainWindow.Instance.LocalizationManager["script_export_format_dll_without_renaming"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(ScriptExportMode value) => value switch
		{
			ScriptExportMode.Decompiled => MainWindow.Instance.LocalizationManager["script_export_format_decompiled_description"],
			ScriptExportMode.Hybrid => MainWindow.Instance.LocalizationManager["not_implemented_yet"],
			ScriptExportMode.DllExportWithRenaming => MainWindow.Instance.LocalizationManager["not_implemented_yet"],
			ScriptExportMode.DllExportWithoutRenaming => MainWindow.Instance.LocalizationManager["script_export_format_dll_without_renaming_description"],
			_ => null,
		};
	}
}
