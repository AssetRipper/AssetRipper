using AssetRipper.Export.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings.DropDown;

public sealed class LightmapTextureExportFormatDropDownSetting : DropDownSetting<LightmapTextureExportFormat>
{
	public static LightmapTextureExportFormatDropDownSetting Instance { get; } = new();

	public override string Title => Localization.LightmapTextureExportTitle;

	protected override string? GetDescription(LightmapTextureExportFormat value)
	{
		return Localization.LightmapTextureFormatDescription;
	}
}
