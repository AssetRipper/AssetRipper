namespace uTinyRipper.Classes.Fonts
{
	public enum FontRenderingMode
	{
		/// <summary>
		/// Use Anti-Aliased Font rendering. When using dynamic fonts, this is the mode which is fastest in rendering font textures.
		/// </summary>
		Smooth = 0,
		/// <summary>
		/// Use Anti-Aliased Font rendering with hinting. This forces character lines to run along pixel boundaries, and generally produces.
		/// </summary>
		HintedSmooth,
		/// <summary>
		/// Use hinted font rendering without anti-aliasing. This is the crispest font rendering option, and may be most readable for small.
		/// </summary>
		HintedRaster,
		/// <summary>
		/// Use the OS default font rendering mode. This selects either FontRenderingMode.HintedSmooth or.
		/// </summary>
		OSDefault,
	}
}
