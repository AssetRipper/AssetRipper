using AssetRipper.Export.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings.DropDown;

public sealed class TextExportModeDropDownSetting : DropDownSetting<TextExportMode>
{
	public static TextExportModeDropDownSetting Instance { get; } = new();

	public override string Title => Localization.TextAssetExportTitle;

	protected override string GetDisplayName(TextExportMode value) => value switch
	{
		TextExportMode.Bytes => Localization.TextAssetFormatBinary,
		TextExportMode.Txt => Localization.TextAssetFormatText,
		TextExportMode.Parse => Localization.TextAssetFormatParse,
		_ => base.GetDisplayName(value),
	};

	protected override string? GetDescription(TextExportMode value) => value switch
	{
		TextExportMode.Bytes => Localization.TextAssetFormatBinaryDescription,
		TextExportMode.Txt => Localization.TextAssetFormatTextDescription,
		TextExportMode.Parse => Localization.TextAssetFormatParseDescription,
		_ => base.GetDescription(value),
	};
}
