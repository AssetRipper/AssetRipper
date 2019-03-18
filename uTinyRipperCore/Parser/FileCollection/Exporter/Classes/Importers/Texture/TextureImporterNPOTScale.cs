namespace uTinyRipper.AssetExporters.Classes
{
	public enum TextureImporterNPOTScale
	{
		/// <summary>
		/// Keep non power of two textures as is.
		/// </summary>
		None			= 0,
		/// <summary>
		/// Scale to nearest power of two.
		/// </summary>
		ToNearest		= 1,
		/// <summary>
		/// Scale to larger power of two.
		/// </summary>
		ToLarger		= 2,
		/// <summary>
		/// Scale to smaller power of two.
		/// </summary>
		ToSmaller		= 3,
	}
}
