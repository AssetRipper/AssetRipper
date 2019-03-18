using System;

namespace uTinyRipperGUI.TextureContainers.DDS
{
	[Flags]
	public enum DDPFFlags : uint
	{
		/// <summary>
		/// Texture contains alpha data; dwRGBAlphaBitMask contains valid data.
		/// </summary>
		DDPF_ALPHAPIXELS	= 0x1,
		/// <summary>
		/// Used in some older DDS files for alpha channel only uncompressed data
		/// (dwRGBBitCount contains the alpha channel bitcount; dwABitMask contains valid data)
		/// </summary>
		DDPF_ALPHA			= 0x2,
		/// <summary>
		/// Texture contains compressed RGB data; dwFourCC contains valid data.
		/// </summary>
		DDPF_FOURCC			= 0x4,
		/// <summary>
		/// Texture contains uncompressed RGB data; dwRGBBitCount and the RGB masks
		/// (dwRBitMask, dwGBitMask, dwBBitMask) contain valid data.
		/// </summary>
		DDPF_RGB			= 0x40,
		/// <summary>
		/// Used in some older DDS files for YUV uncompressed data
		/// (dwRGBBitCount contains the YUV bit count; dwRBitMask contains the Y mask,
		/// dwGBitMask contains the U mask, dwBBitMask contains the V mask)
		/// </summary>
		DDPF_YUV			= 0x200,
		/// <summary>
		/// Used in some older DDS files for single channel color uncompressed data
		/// (dwRGBBitCount contains the luminance channel bit count; dwRBitMask contains the channel mask).
		/// Can be combined with DDPF_ALPHAPIXELS for a two channel DDS file.
		/// </summary>
		DDPF_LUMINANCE = 0x20000,
	}

	public static class DDPFFlagsExtensions
	{
		public static bool IsAlphaPixels(this DDPFFlags _this)
		{
			return (_this & DDPFFlags.DDPF_ALPHAPIXELS) != 0;
		}

		public static bool IsAlpha(this DDPFFlags _this)
		{
			return (_this & DDPFFlags.DDPF_ALPHA) != 0;
		}

		public static bool IsFourCC(this DDPFFlags _this)
		{
			return (_this & DDPFFlags.DDPF_FOURCC) != 0;
		}

		public static bool IsRGB(this DDPFFlags _this)
		{
			return (_this & DDPFFlags.DDPF_RGB) != 0;
		}

		public static bool IsYUV(this DDPFFlags _this)
		{
			return (_this & DDPFFlags.DDPF_YUV) != 0;
		}

		public static bool IsLuminace(this DDPFFlags _this)
		{
			return (_this & DDPFFlags.DDPF_LUMINANCE) != 0;
		}
	}
}
