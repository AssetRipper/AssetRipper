using AssetRipper.Export.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings.DropDown;

public sealed class ScriptExportModeDropDownSetting : DropDownSetting<ScriptExportMode>
{
	public static ScriptExportModeDropDownSetting Instance { get; } = new();

	public override string Title => Localization.ScriptExportTitle;

	protected override string GetDisplayName(ScriptExportMode value) => value switch
	{
		ScriptExportMode.Decompiled => Localization.ScriptExportFormatDecompiled,
		ScriptExportMode.Hybrid => Localization.ScriptExportFormatHybrid,
		ScriptExportMode.DllExportWithRenaming => Localization.ScriptExportFormatDllWithRenaming,
		ScriptExportMode.DllExportWithoutRenaming => Localization.ScriptExportFormatDllWithoutRenaming,
		_ => base.GetDisplayName(value),
	};

	protected override string? GetDescription(ScriptExportMode value) => value switch
	{
		ScriptExportMode.Decompiled => Localization.ScriptExportFormatDecompiledDescription,
		ScriptExportMode.Hybrid => Localization.ScriptExportFormatHybridDescription,
		ScriptExportMode.DllExportWithRenaming => Localization.NotImplementedYet,
		ScriptExportMode.DllExportWithoutRenaming => Localization.ScriptExportFormatDllWithoutRenamingDescription,
		_ => base.GetDescription(value),
	};
}
