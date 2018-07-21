namespace UtinyRipper.Classes.Textures
{
	public enum TextureUsageMode
	{
		Default = 0,
		BakedLightmapDoubleLDR = 1,
		BakedLightmapRGBM = 2,
		NormalmapDXT5nm = 3,
		NormalmapPlain = 4,
		RGBMEncoded = 5,
		AlwaysPadded = 6,
		DoubleLDR = 7,
		BakedLightmapFullHDR = 8,
		RealtimeLightmapRGBM = 9,
	}

	public static class TextureUsageModeExtensions
	{
		public static bool IsNormalmap(this TextureUsageMode _this)
		{
			return _this == TextureUsageMode.NormalmapDXT5nm || _this == TextureUsageMode.NormalmapPlain;
		}
	}
}
