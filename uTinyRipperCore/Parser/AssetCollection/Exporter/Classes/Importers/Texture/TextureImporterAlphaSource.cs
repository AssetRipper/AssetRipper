namespace uTinyRipper.AssetExporters.Classes
{
	/// <summary>
	/// Select how the alpha of the imported texture is generated.
	/// </summary>
	public enum TextureImporterAlphaSource
	{
		/// <summary>
		/// No Alpha will be used.
		/// </summary>
		None			= 0,
		/// <summary>
		/// Use Alpha from the input texture if one is provided.
		/// </summary>
		FromInput		= 1,
		/// <summary>
		/// Generate Alpha from image gray scale.
		/// </summary>
		FromGrayScale	= 2,
	}
}
