namespace uTinyRipper.Classes.GraphicsSettingss
{
	/// <summary>
	/// The HDR mode to use for rendering.
	/// </summary>
	public enum CameraHDRMode
	{
		/// <summary>
		/// Uses RenderTextureFormat.ARGBHalf.
		/// </summary>
		FP16		= 1,
		/// <summary>
		/// Uses RenderTextureFormat.RGB111110Float.
		/// </summary>
		R11G11B10	= 2,
	}
}
