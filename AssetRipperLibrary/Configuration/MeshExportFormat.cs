namespace AssetRipper.Library.Configuration
{
	public enum MeshExportFormat
	{
		/// <summary>
		/// A robust format for using meshes in the editor. Can be converted to other formats by a variety of unity packages.
		/// </summary>
		Native,
		/// <summary>
		/// A common mesh format usuable in a variety of applications. However, this also breaks exported references to the mesh asset.
		/// </summary>
		Obj,
	}
}