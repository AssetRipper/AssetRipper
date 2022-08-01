namespace AssetRipper.Library.Configuration
{
	public enum TerrainExportMode
	{
		/// <summary>
		/// The default export mode. This is the only one that exports in a format Unity can use for terrains.
		/// </summary>
		Native,
		/// <summary>
		/// This experimental mode converts the terrain data into an OBJ mesh. Exported mesh size, position, and proportions may not be correct.
		/// </summary>
		Obj,
		/// <summary>
		/// A heatmap of the terrain height. Probably not usable for anything but a visual representation.
		/// </summary>
		Heatmap,
	}
}
