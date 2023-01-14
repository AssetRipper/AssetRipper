using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class TextureFormatExtensions
{
	public static bool IsCrunched(this TextureFormat format)
	{
		return format is TextureFormat.DXT1Crunched or TextureFormat.DXT5Crunched or TextureFormat.ETC_RGB4Crunched or TextureFormat.ETC2_RGBA8Crunched;
	}
}
