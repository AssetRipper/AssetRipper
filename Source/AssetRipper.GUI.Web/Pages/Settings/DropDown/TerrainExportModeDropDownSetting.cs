using AssetRipper.Export.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings.DropDown;

public sealed class TerrainExportModeDropDownSetting : DropDownSetting<TerrainExportMode>
{
	public static TerrainExportModeDropDownSetting Instance { get; } = new();

	public override string Title => Localization.TerrainExportTitle;

	protected override string GetDisplayName(TerrainExportMode value) => value switch
	{
		TerrainExportMode.Yaml => Localization.TerrainFormatNative,
		TerrainExportMode.Mesh => Localization.TerrainFormatMesh,
		TerrainExportMode.Heatmap => Localization.TerrainFormatHeatmap,
		_ => base.GetDisplayName(value),
	};

	protected override string? GetDescription(TerrainExportMode value) => value switch
	{
		TerrainExportMode.Yaml => Localization.TerrainFormatNativeDescription,
		TerrainExportMode.Mesh => Localization.TerrainFormatMeshDescription,
		TerrainExportMode.Heatmap => Localization.TerrainFormatHeatmapDescription,
		_ => base.GetDescription(value),
	};
}
