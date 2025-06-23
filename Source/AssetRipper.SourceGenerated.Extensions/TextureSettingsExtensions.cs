using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.TextureSettings;
using FilterMode = AssetRipper.SourceGenerated.Enums.FilterMode_0;

namespace AssetRipper.SourceGenerated.Extensions;

public static class TextureSettingsExtensions
{
	public static TextureImporterCompression GetTextureCompression(this ITextureSettings settings)
	{
		return (TextureImporterCompression)settings.TextureCompression;
	}

	public static void SetTextureCompression(this ITextureSettings settings, TextureImporterCompression compression)
	{
		settings.TextureCompression = (int)compression;
	}

	public static FilterMode GetFilterMode(this ITextureSettings settings)
	{
		return (FilterMode)settings.FilterMode;
	}

	public static void SetFilterMode(this ITextureSettings settings, FilterMode filterMode)
	{
		settings.FilterMode = (int)filterMode;
	}

	public static ColorSpace GetColorSpace(this ITextureSettings settings)
	{
		return settings.Has_ColorSpace()
			? (ColorSpace)settings.ColorSpace
			: settings.Has_SRGB_Boolean()
				? settings.SRGB_Boolean ? ColorSpace.Linear : ColorSpace.Gamma //Not 100% sure on this
				: (ColorSpace)settings.SRGB_Int32; //Not 100% sure on this
	}

	public static bool GetSRGB(this ITextureSettings settings)
	{
		return settings.Has_SRGB_Boolean()
			? settings.SRGB_Boolean
			: settings.Has_SRGB_Int32()
				? settings.SRGB_Int32 != 0
				: settings.ColorSpace != (int)ColorSpace.Gamma; //Not 100% sure on this
	}

	public static void SetSRGB(this ITextureSettings settings, bool value)
	{
		settings.SRGB_Boolean = value;
		settings.SRGB_Int32 = value ? 1 : 0;
		settings.ColorSpace = value ? (int)ColorSpace.Linear : (int)ColorSpace.Gamma;
	}

	public static int GetAnisoLevel(this ITextureSettings settings)
	{
		return settings.Has_AnisoLevel_Int32()
			? settings.AnisoLevel_Int32
			: unchecked((int)settings.AnisoLevel_UInt32);
	}

	public static void SetAnisoLevel(this ITextureSettings settings, int value)
	{
		settings.AnisoLevel_Int32 = value;
		settings.AnisoLevel_UInt32 = unchecked((uint)value);
	}

	public static int GetCompressionQuality(this ITextureSettings settings)
	{
		return settings.Has_CompressionQuality_Int32()
			? settings.CompressionQuality_Int32
			: unchecked((int)settings.CompressionQuality_UInt32);
	}

	public static void SetCompressionQuality(this ITextureSettings settings, int value)
	{
		settings.CompressionQuality_Int32 = value;
		settings.CompressionQuality_UInt32 = unchecked((uint)value);
	}

	public static int GetMaxTextureSize(this ITextureSettings settings)
	{
		return settings.Has_MaxTextureSize_Int32()
			? settings.MaxTextureSize_Int32
			: unchecked((int)settings.MaxTextureSize_UInt32);
	}

	public static void SetMaxTextureSize(this ITextureSettings settings, int value)
	{
		settings.MaxTextureSize_Int32 = value;
		settings.MaxTextureSize_UInt32 = unchecked((uint)value);
	}

	public static bool GetGenerateMipMaps(this ITextureSettings settings)
	{
		return settings.Has_GenerateMipMaps_Int32()
			? settings.GenerateMipMaps_Int32 != 0
			: settings.GenerateMipMaps_Boolean;
	}

	public static void SetGenerateMipMaps(this ITextureSettings settings, bool value)
	{
		settings.GenerateMipMaps_Int32 = value ? 1 : 0;
		settings.GenerateMipMaps_Boolean = value;
	}

	public static bool GetReadable(this ITextureSettings settings)
	{
		return settings.Has_Readable_Int32()
			? settings.Readable_Int32 != 0
			: settings.Readable_Boolean;
	}

	public static void SetReadable(this ITextureSettings settings, bool value)
	{
		settings.Readable_Int32 = value ? 1 : 0;
		settings.Readable_Boolean = value;
	}

	public static bool GetCrunchedCompression(this ITextureSettings settings)
	{
		return settings.Has_CrunchedCompression_Int32()
			? settings.CrunchedCompression_Int32 != 0
			: settings.CrunchedCompression_Boolean;
	}

	public static void SetCrunchedCompression(this ITextureSettings settings, bool value)
	{
		settings.CrunchedCompression_Int32 = value ? 1 : 0;
		settings.CrunchedCompression_Boolean = value;
	}

	public static void Initialize(this ITextureSettings settings)
	{
		settings.SetAnisoLevel(1);
		settings.SetCompressionQuality(50);
		settings.SetMaxTextureSize(2048);
		settings.SetTextureCompression(TextureImporterCompression.Uncompressed);
		settings.SetFilterMode(FilterMode.Bilinear);
		settings.SetGenerateMipMaps(false);
		settings.SetReadable(false);
		settings.SetCrunchedCompression(false);
		settings.SetSRGB(true);
	}
}
