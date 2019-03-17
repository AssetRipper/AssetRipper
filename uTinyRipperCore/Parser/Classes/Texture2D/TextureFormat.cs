using System;

namespace uTinyRipper.Classes.Textures
{
	public enum TextureFormat
	{
		PVRTC_2BPP_RGB = -127,
		PVRTC_2BPP_RGBA = -127,
		PVRTC_4BPP_RGB = -127,
		PVRTC_4BPP_RGBA = -127,
		/// <summary>
		/// Choose a compressed HDR format automatically.
		/// </summary>
		AutomaticCompressedHDR = -7,
		/// <summary>
		/// Choose an HDR format automatically.
		/// </summary>
		AutomaticHDR = -6,
		/// <summary>
		/// Choose a crunched format automatically.
		/// </summary>
		AutomaticCrunched = -5,
		/// <summary>
		/// Choose a Truecolor format automatically.
		/// </summary>
		AutomaticTruecolor = -3,
		/// <summary>
		/// Choose a 16 bit format automatically.
		/// </summary>
		Automatic16bit = -2,
		/// <summary>
		/// Choose texture format automatically based on the texture parameters.
		/// </summary>
		Automatic = -1,
		/// <summary>
		/// Choose a compressed format automatically.
		/// </summary>
		AutomaticCompressed = -1,
		/// <summary>
		/// Alpha-only texture format.
		/// </summary>
		Alpha8 = 1,
		/// <summary>
		/// A 16 bits/pixel texture format. Texture stores color with an alpha channel.
		/// </summary>
		ARGB4444 = 2,
		/// <summary>
		/// Color texture format, 8-bits per channel.
		/// </summary>
		RGB24 = 3,
		/// <summary>
		/// Color with alpha texture format, 8-bits per channel.
		/// </summary>
		RGBA32 = 4,
		/// <summary>
		/// Color with alpha texture format, 8-bits per channel.
		/// </summary>
		ARGB32 = 5,
		/// <summary>
		/// A 16 bit color texture format.
		/// </summary>
		RGB565 = 7,
		/// <summary>
		/// A 16 bit color texture format that only has a red channel.
		/// </summary>
		R16 = 9,
		/// <summary>
		/// Compressed color texture format.
		/// </summary>
		DXT1 = 10,
		DXT3 = 11,
		/// <summary>
		/// Compressed color with alpha channel texture format.
		/// </summary>
		DXT5 = 12,
		/// <summary>
		/// Color and alpha texture format, 4 bit per channel.
		/// </summary>
		RGBA4444 = 13,
		/// <summary>
		/// Color with alpha texture format, 8-bits per channel.
		/// </summary>
		BGRA32 = 14,
		/// <summary>
		/// Scalar (R) texture format, 16 bit floating point.
		/// </summary>
		RHalf = 15,
		/// <summary>
		/// Two color (RG) texture format, 16 bit floating point per channel.
		/// </summary>
		RGHalf = 16,
		/// <summary>
		/// RGB color and alpha texture format, 16 bit floating point per channel.
		/// </summary>
		RGBAHalf = 17,
		/// <summary>
		/// Scalar (R) texture format, 32 bit floating point.
		/// </summary>
		RFloat = 18,
		/// <summary>
		/// Two color (RG) texture format, 32 bit floating point per channel.
		/// </summary>
		RGFloat = 19,
		/// <summary>
		/// RGB color and alpha texture format, 32-bit floats per channel.
		/// </summary>
		RGBAFloat = 20,
		/// <summary>
		/// A format that uses the YUV color space and is often used for video encoding or playback.
		/// </summary>
		YUY2 = 21,
		/// <summary>
		/// RGB HDR format, with 9 bit mantissa per channel and a 5 bit shared exponent.
		/// </summary>
		RGB9e5Float = 22,
		/// <summary>
		/// HDR compressed color texture format.
		/// </summary>
		BC6H = 24,
		/// <summary>
		/// High quality compressed color texture format.
		/// </summary>
		BC7 = 25,
		/// <summary>
		/// Compressed one channel (R) texture format.
		/// </summary>
		BC4 = 26,
		/// <summary>
		/// Compressed two-channel (RG) texture format.
		/// </summary>
		BC5 = 27,
		/// <summary>
		/// Compressed color texture format with Crunch compression for smaller storage sizes.
		/// </summary>
		DXT1Crunched = 28,
		/// <summary>
		/// Compressed color with alpha channel texture format with Crunch compression for smaller storage sizes.
		/// </summary>
		DXT5Crunched = 29,
		/// <summary>
		/// PowerVR (iOS) 2 bits/pixel compressed color texture format.
		/// </summary>
		PVRTC_RGB2 = 30,
		/// <summary>
		/// PowerVR (iOS) 2 bits/pixel compressed with alpha channel texture format.
		/// </summary>
		PVRTC_RGBA2 = 31,
		/// <summary>
		/// PowerVR (iOS) 4 bits/pixel compressed color texture format.
		/// </summary>
		PVRTC_RGB4 = 32,
		/// <summary>
		/// PowerVR (iOS) 4 bits/pixel compressed with alpha channel texture format.
		/// </summary>
		PVRTC_RGBA4 = 33,
		/// <summary>
		/// ETC (GLES2.0) 4 bits/pixel compressed RGB texture format.
		/// </summary>
		ETC_RGB4 = 34,
		/// <summary>
		/// ATC (ATITC) 4 bits/pixel compressed RGB texture format.
		/// </summary>
		ATC_RGB4 = 35,
		/// <summary>
		/// ATC (ATITC) 8 bits/pixel compressed RGB texture format.
		/// </summary>
		ATC_RGBA8 = 36,
		/// <summary>
		/// ETC2 EAC (GL ES 3.0) 4 bitspixel compressed unsigned single-channel texture format.
		/// </summary>
		EAC_R = 41,
		/// <summary>
		/// ETC2 EAC (GL ES 3.0) 4 bitspixel compressed signed single-channel texture format.
		/// </summary>
		EAC_R_SIGNED = 42,
		/// <summary>
		/// ETC2 EAC (GL ES 3.0) 8 bitspixel compressed unsigned dual-channel (RG) texture format.
		/// </summary>
		EAC_RG = 43,
		/// <summary>
		/// ETC2 EAC (GL ES 3.0) 8 bitspixel compressed signed dual-channel (RG) texture format.
		/// </summary>
		EAC_RG_SIGNED = 44,
		/// <summary>
		/// ETC2 (GL ES 3.0) 4 bits/pixel compressed RGB texture format.
		/// </summary>
		ETC2_RGB = 45,
		/// <summary>
		/// ETC2 (GL ES 3.0) 4 bits/pixel RGB+1-bit alpha texture format.
		/// </summary>
		ETC2_RGBA1 = 46,
		/// <summary>
		/// ETC2 (GL ES 3.0) 8 bits/pixel compressed RGBA texture format.
		/// </summary>
		ETC2_RGBA8 = 47,
		/// <summary>
		/// ASTC (4x4 pixel block in 128 bits) compressed RGB texture format.
		/// </summary>
		ASTC_RGB_4x4 = 48,
		/// <summary>
		/// ASTC (5x5 pixel block in 128 bits) compressed RGB texture format.
		/// </summary>
		ASTC_RGB_5x5 = 49,
		/// <summary>
		/// ASTC (6x6 pixel block in 128 bits) compressed RGB texture format.
		/// </summary>
		ASTC_RGB_6x6 = 50,
		/// <summary>
		/// ASTC (8x8 pixel block in 128 bits) compressed RGB texture format.
		/// </summary>
		ASTC_RGB_8x8 = 51,
		/// <summary>
		/// ASTC (10x10 pixel block in 128 bits) compressed RGB texture format.
		/// </summary>
		ASTC_RGB_10x10 = 52,
		/// <summary>
		/// ASTC (12x12 pixel block in 128 bits) compressed RGB texture format.
		/// </summary>
		ASTC_RGB_12x12 = 53,
		/// <summary>
		/// ASTC (4x4 pixel block in 128 bits) compressed RGBA texture format.
		/// </summary>
		ASTC_RGBA_4x4 = 54,
		/// <summary>
		/// ASTC (5x5 pixel block in 128 bits) compressed RGBA texture format.
		/// </summary>
		ASTC_RGBA_5x5 = 55,
		/// <summary>
		/// ASTC (6x6 pixel block in 128 bits) compressed RGBA texture format.
		/// </summary>
		ASTC_RGBA_6x6 = 56,
		/// <summary>
		/// ASTC (8x8 pixel block in 128 bits) compressed RGBA texture format.
		/// </summary>
		ASTC_RGBA_8x8 = 57,
		/// <summary>
		/// ASTC (10x10 pixel block in 128 bits) compressed RGBA texture format.
		/// </summary>
		ASTC_RGBA_10x10 = 58,
		/// <summary>
		/// ASTC (12x12 pixel block in 128 bits) compressed RGBA texture format.
		/// </summary>
		ASTC_RGBA_12x12 = 59,
		/// <summary>
		/// ETC 4 bits/pixel compressed RGB texture format.
		/// </summary>
		ETC_RGB4_3DS = 60,
		/// <summary>
		/// ETC 4 bitspixel RGB + 4 bitspixel Alpha compressed texture format.
		/// </summary>
		ETC_RGBA8_3DS = 61,
		/// <summary>
		/// Two color (RG) texture format, 8-bits per channel.
		/// </summary>
		RG16 = 62,
		/// <summary>
		/// Scalar (R) render texture format, 8 bit fixed point.
		/// </summary>
		R8 = 63,
		/// <summary>
		/// Compressed color texture format with Crunch compression for smaller storage sizes.
		/// </summary>
		ETC_RGB4Crunched = 64,
		/// <summary>
		/// Compressed color with alpha channel texture format with Crunch compression for smaller storage sizes.
		/// </summary>
		ETC2_RGBA8Crunched = 65
	}

	public static class TextureFormatExtensions
	{
		public static TextureFormat ToDefaultFormat(this TextureFormat _this)
		{
			if (_this == TextureFormat.R8)
			{
				return _this;
			}
			return TextureFormat.Automatic;
		}
	}
}
