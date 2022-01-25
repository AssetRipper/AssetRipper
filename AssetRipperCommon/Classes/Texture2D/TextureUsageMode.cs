namespace AssetRipper.Core.Classes.Texture2D
{
	/// <summary>
	/// Lightmap format of a [[Texture2D|texture]]<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/AssetPipeline/TextureImporterEnums.cs"/>
	/// </summary>
	public enum TextureUsageMode
	{
		/// <summary>
		/// Not a lightmap
		/// </summary>
		Default = 0,
		/// <summary>
		/// Range [0;2] packed to [0;1] with loss of precision
		/// </summary>
		BakedLightmapDoubleLDR = 1,
		/// <summary>
		/// Range [0;kLightmapRGBMMax] packed to [0;1] with multiplier stored in the alpha channel
		/// </summary>
		BakedLightmapRGBM = 2,
		/// <summary>
		/// Compressed DXT5 normal map
		/// </summary>
		NormalmapDXT5nm = 3,
		/// <summary>
		/// Plain RGB normal map
		/// </summary>
		NormalmapPlain = 4,
		RGBMEncoded = 5,
		/// <summary>
		/// Texture is always padded if NPOT and on low-end hardware
		/// </summary>
		AlwaysPadded = 6,
		DoubleLDR = 7,
		/// <summary>
		/// Baked lightmap without any encoding
		/// </summary>
		BakedLightmapFullHDR = 8,
		RealtimeLightmapRGBM = 9,
		NormalmapASTCnm = 10,
		SingleChannelRed = 11,
		SingleChannelAlpha = 12,
	}

	public static class TextureUsageModeExtensions
	{
		public static bool IsNormalmap(this TextureUsageMode _this)
		{
			return _this == TextureUsageMode.NormalmapDXT5nm || _this == TextureUsageMode.NormalmapPlain || _this == TextureUsageMode.NormalmapASTCnm;
		}
	}
}
