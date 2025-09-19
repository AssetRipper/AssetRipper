namespace AssetRipper.Export.Configuration;

public enum TerrainExportMode
{
	/// <summary>
	/// The default export mode. This is the only one that exports in a format Unity can use for terrains.
	/// </summary>
	Yaml,
	/// <summary>
	/// This converts the terrain data into a mesh. Unity cannot import this.
	/// </summary>
	Mesh,
	/// <summary>
	/// A heatmap of the terrain height. Probably not usable for anything but a visual representation.
	/// </summary>
	Heatmap,
}
