namespace AssetRipper.Core.Classes.GraphicsSettings
{
	/// <summary>
	/// The HDR mode to use for rendering.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum CameraHDRMode
	{
		/// <summary>
		/// Uses RenderTextureFormat.ARGBHalf.
		/// </summary>
		FP16 = 1,
		/// <summary>
		/// Uses RenderTextureFormat.RGB111110Float.
		/// </summary>
		R11G11B10 = 2,
	}
}
