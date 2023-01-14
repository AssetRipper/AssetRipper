using AssetRipper.Export.UnityProjects.Configuration;

namespace AssetRipper.GUI.Components
{
	public class TerrainExportConfigDropdown : BaseConfigurationDropdown<TerrainExportMode>
	{
		protected override string GetValueDisplayName(TerrainExportMode value) => value switch
		{
			TerrainExportMode.Yaml => MainWindow.Instance.LocalizationManager["terrain_format_native"],
			TerrainExportMode.Mesh => MainWindow.Instance.LocalizationManager["terrain_format_mesh"],
			TerrainExportMode.Heatmap => MainWindow.Instance.LocalizationManager["terrain_format_heatmap"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(TerrainExportMode value) => value switch
		{
			TerrainExportMode.Yaml => MainWindow.Instance.LocalizationManager["terrain_format_native_description"],
			TerrainExportMode.Mesh => MainWindow.Instance.LocalizationManager["terrain_format_mesh_description"],
			TerrainExportMode.Heatmap => MainWindow.Instance.LocalizationManager["terrain_format_heatmap_description"],
			_ => null,
		};
	}
}
