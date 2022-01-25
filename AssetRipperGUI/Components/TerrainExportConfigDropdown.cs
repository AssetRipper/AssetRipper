using AssetRipper.Library.Configuration;

namespace AssetRipper.GUI.Components
{
	public class TerrainExportConfigDropdown : BaseConfigurationDropdown<TerrainExportMode>
	{
		protected override string GetValueDisplayName(TerrainExportMode value) => value switch
		{
			TerrainExportMode.Native => MainWindow.Instance.LocalizationManager["terrain_format_native"],
			TerrainExportMode.Obj => MainWindow.Instance.LocalizationManager["terrain_format_obj"],
			TerrainExportMode.Heatmap => MainWindow.Instance.LocalizationManager["terrain_format_heatmap"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(TerrainExportMode value) => value switch
		{
			TerrainExportMode.Native => MainWindow.Instance.LocalizationManager["terrain_format_native_description"],
			TerrainExportMode.Obj => MainWindow.Instance.LocalizationManager["terrain_format_obj_description"],
			TerrainExportMode.Heatmap => MainWindow.Instance.LocalizationManager["terrain_format_heatmap_description"],
			_ => null,
		};
	}
}