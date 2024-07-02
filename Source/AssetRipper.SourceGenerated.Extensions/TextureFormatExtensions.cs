using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class TextureFormatExtensions
{
	public static bool IsCrunched(this TextureFormat format)
	{
		return format is TextureFormat.DXT1Crunched or TextureFormat.DXT5Crunched or TextureFormat.ETC_RGB4Crunched or TextureFormat.ETC2_RGBA8Crunched;
	}

	public static bool IsDxt(this TextureFormat format)
	{
		return format is TextureFormat.DXT1 or TextureFormat.DXT1Crunched or TextureFormat.DXT3 or TextureFormat.DXT5 or TextureFormat.DXT5Crunched;
	}

	public static bool IsRgb(this TextureFormat format)
	{
		return format
			is TextureFormat.Alpha8
			or TextureFormat.ARGB4444
			or TextureFormat.RGB24
			or TextureFormat.RGBA32
			or TextureFormat.ARGB32
			or TextureFormat.RGB565
			or TextureFormat.R16
			or TextureFormat.RGBA4444
			or TextureFormat.BGRA32_14
			or TextureFormat.RHalf
			or TextureFormat.RGHalf
			or TextureFormat.RGBAHalf
			or TextureFormat.RFloat
			or TextureFormat.RGFloat
			or TextureFormat.RGBAFloat
			or TextureFormat.RGB9e5Float
			or TextureFormat.BGRA32_37
			or TextureFormat.RG16
			or TextureFormat.R8
			or TextureFormat.RG32
			or TextureFormat.RGB48
			or TextureFormat.RGBA64
			or TextureFormat.R8_SIGNED
			or TextureFormat.RG16_SIGNED
			or TextureFormat.RGB24_SIGNED
			or TextureFormat.RGBA32_SIGNED
			or TextureFormat.R16_SIGNED
			or TextureFormat.RG32_SIGNED
			or TextureFormat.RGB48_SIGNED
			or TextureFormat.RGBA64_SIGNED;
	}
}
