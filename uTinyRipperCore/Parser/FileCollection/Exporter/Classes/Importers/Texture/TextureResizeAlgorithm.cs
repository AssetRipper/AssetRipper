namespace uTinyRipper.AssetExporters.Classes
{
	/// <summary>
	/// For Texture to be scaled down choose resize algorithm.
	/// ( Applyed only when Texture dimension is bigger than Max Size ).
	/// </summary>
	public enum TextureResizeAlgorithm
	{
		/// <summary>
		/// Default high quality resize algorithm.
		/// </summary>
		Mitchell = 0,
		/// <summary>
		/// Might provide better result than Mitchell for some noise textures preserving more sharp details.
		/// </summary>
		Bilinear = 1,
	}
}
