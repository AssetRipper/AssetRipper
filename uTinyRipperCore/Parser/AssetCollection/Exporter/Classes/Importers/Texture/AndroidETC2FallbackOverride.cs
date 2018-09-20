namespace uTinyRipper.AssetExporters.Classes
{
	/// <summary>
	/// This enumeration has values for different qualities to decompress an ETC2 texture on Android devices
	/// that don't support the ETC2 texture format.
	/// </summary>
	public enum AndroidETC2FallbackOverride
	{
		/// <summary>
		/// Use the value defined in Player build settings.
		/// </summary>
		UseBuildSettings = 0,
		/// <summary>
		/// Texture is decompressed to the TextureFormat.RGBA32 format.
		/// </summary>
		Quality32Bit = 1,
		/// <summary>
		/// Texture is decompressed to a suitable 16-bit format.
		/// </summary>
		Quality16Bit = 2,
		/// <summary>
		/// Texture is decompressed to the TextureFormat.RGBA32 format and downscaled to half of the original texture width and height.
		/// </summary>
		Quality32BitDownscaled = 3,
	}
}
