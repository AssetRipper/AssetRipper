namespace uTinyRipper.AssetExporters.Classes
{
	/// <summary>
	/// Select the kind of compression you want for your texture.
	/// </summary>
	public enum TextureImporterCompression
	{
		/// <summary>
		/// Texture will not be compressed.
		/// </summary>
		Uncompressed = 0,
		/// <summary>
		/// Texture will be compressed using a standard format depending on the platform (DXT, ASTC, ...).
		/// </summary>
		Compressed = 1,
		/// <summary>
		/// Texture will be compressed using a high quality format depending on the platform and availability (BC7, ASTC4x4, ...).
		/// </summary>
		CompressedHQ = 2,
		/// <summary>
		/// Texture will be compressed using a low quality but high performance, high compression format depending on the platform and availability (2bpp PVRTC, ASTC8x8, ...).
		/// </summary>
		CompressedLQ = 3,
	}
}
