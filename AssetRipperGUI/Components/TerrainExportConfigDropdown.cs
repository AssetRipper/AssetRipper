using AssetRipper.Library.Configuration;

namespace AssetRipper.GUI.Components
{
	public class TerrainExportConfigDropdown : BaseConfigurationDropdown<TerrainExportMode>
	{
		protected override string GetValueDisplayName(TerrainExportMode value) => value switch
		{
			TerrainExportMode.Native => "Unity",
			TerrainExportMode.Obj => "3D Model (OBJ)",
			TerrainExportMode.Heatmap => "Heightmap",
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(TerrainExportMode value)  => value switch
		{
			TerrainExportMode.Native => "Export in the native unity terrain format. Most useful option if you plan to re-import back into unity.",
			TerrainExportMode.Heatmap => "Export a heatmap of the height of the terrain at each location. Only really useful if you don't care about the details or having the terrain in 3D.",
			TerrainExportMode.Obj => "Export the terrain as a 3D model in OBJ format, suitable for viewing with a wide range of 3D editors",
			_ => null,
		};
	}
}