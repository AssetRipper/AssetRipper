using System;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Textures;
using uTinyRipperGUI.TextureContainers.DDS;
using uTinyRipperGUI.TextureContainers.KTX;
using uTinyRipperGUI.TextureContainers.PVR;

using KTXFormat = uTinyRipperGUI.TextureContainers.KTX.KTXInternalFormat;
using KTXBaseFormat = uTinyRipperGUI.TextureContainers.KTX.KTXBaseInternalFormat;
using PVRFormat = uTinyRipperGUI.TextureContainers.PVR.PVRPixelFormat;

namespace uTinyRipperGUI.Exporters
{
	public static class Texture2DExtensions
	{
		public static DDSCapsFlags DDSCaps(this Texture2D _this)
		{
			if (Texture2D.IsBoolMinMap(_this.File.Version))
			{
				if (_this.MipCount <= 1)
				{
					return DDSCapsFlags.DDSCAPS_TEXTURE;
				}
			}
			return DDSCapsFlags.DDSCAPS_TEXTURE | DDSCapsFlags.DDSCAPS_MIPMAP | DDSCapsFlags.DDSCAPS_COMPLEX;
		}

		public static bool DDSIsPitchOrLinearSize(this Texture2D _this)
		{
			if (_this.MipCount > 1)
			{
				switch (_this.TextureFormat)
				{
					case TextureFormat.DXT1:
					case TextureFormat.DXT1Crunched:
					case TextureFormat.DXT3:
					case TextureFormat.DXT5:
					case TextureFormat.DXT5Crunched:
						return true;
				}
			}
			return false;
		}

		public static DDPFFlags DDSPixelFormatFlags(this Texture2D _this)
		{
			switch (_this.TextureFormat)
			{
				case TextureFormat.Alpha8:
				case TextureFormat.ARGB4444:
				case TextureFormat.RGBA32:
				case TextureFormat.ARGB32:
				case TextureFormat.RGBA4444:
				case TextureFormat.BGRA32:
					return DDPFFlags.DDPF_RGB | DDPFFlags.DDPF_ALPHAPIXELS;

				case TextureFormat.R8:
				case TextureFormat.R16:
				case TextureFormat.RGB24:
				case TextureFormat.RGB565:
				case TextureFormat.RG16:
					return DDPFFlags.DDPF_RGB;

				case TextureFormat.DXT1:
				case TextureFormat.DXT1Crunched:
				case TextureFormat.DXT3:
				case TextureFormat.DXT5:
				case TextureFormat.DXT5Crunched:
					return DDPFFlags.DDPF_FOURCC;

				default:
					throw new NotSupportedException($"Texture format {_this.TextureFormat} isn't supported");
			}
		}

		public static uint DDSFourCC(this Texture2D _this)
		{
			switch (_this.TextureFormat)
			{
				case TextureFormat.DXT1:
				case TextureFormat.DXT1Crunched:
					// ASCII - 'DXT1'
					return 0x31545844;
				case TextureFormat.DXT3:
					// ASCII - 'DXT3'
					return 0x33545844;
				case TextureFormat.DXT5:
				case TextureFormat.DXT5Crunched:
					// ASCII - 'DXT5'
					return 0x35545844;

				default:
					return 0;
			}
		}

		public static int DDSRGBBitCount(this Texture2D _this)
		{
			switch (_this.TextureFormat)
			{
				case TextureFormat.RGBA32:
					return 32;
				case TextureFormat.ARGB32:
					return 32;
				case TextureFormat.BGRA32:
					return 32;

				case TextureFormat.ARGB4444:
					return 16;
				case TextureFormat.RGBA4444:
					return 16;

				case TextureFormat.RGB24:
					return 24;

				case TextureFormat.R8:
				case TextureFormat.Alpha8:
					return 8;

				case TextureFormat.R16:
				case TextureFormat.RG16:
					return 16;

				case TextureFormat.RGB565:
					return 16;

				case TextureFormat.DXT1:
				case TextureFormat.DXT1Crunched:
				case TextureFormat.DXT3:
				case TextureFormat.DXT5:
				case TextureFormat.DXT5Crunched:
					return 0;

				default:
					throw new NotSupportedException($"Texture format {_this.TextureFormat} isn't supported");
			}
		}

		public static uint DDSRBitMask(this Texture2D _this)
		{
			switch (_this.TextureFormat)
			{
				case TextureFormat.RGBA32:
					return 0x000000FF;
				case TextureFormat.BGRA32:
					return 0x00FF0000;
				case TextureFormat.ARGB32:
					return 0x0000FF00;

				case TextureFormat.ARGB4444:
					return 0x0F00;
				case TextureFormat.RGBA4444:
					return 0xF000;

				case TextureFormat.RGB24:
					return 0x0000FF;

				case TextureFormat.RGB565:
					return 0xF800;

				case TextureFormat.Alpha8:
					return 0;

				case TextureFormat.R8:
					return 0xFF;
				case TextureFormat.R16:
					return 0xFFFF;
				case TextureFormat.RG16:
					return 0xFF;

				case TextureFormat.DXT1:
				case TextureFormat.DXT1Crunched:
				case TextureFormat.DXT3:
				case TextureFormat.DXT5:
				case TextureFormat.DXT5Crunched:
					return 0;

				default:
					throw new NotSupportedException($"Texture format {_this.TextureFormat} isn't supported");
			}
		}

		public static uint DDSGBitMask(this Texture2D _this)
		{
			switch (_this.TextureFormat)
			{
				case TextureFormat.RGBA32:
					return 0x0000FF00;
				case TextureFormat.BGRA32:
					return 0x0000FF00;
				case TextureFormat.ARGB32:
					return 0x00FF0000;

				case TextureFormat.ARGB4444:
					return 0x00F0;
				case TextureFormat.RGBA4444:
					return 0x0F00;

				case TextureFormat.RGB24:
					return 0xFF00;

				case TextureFormat.RGB565:
					return 0x07E0;

				case TextureFormat.Alpha8:

					return 0;
				case TextureFormat.R8:
					return 0;
				case TextureFormat.R16:
					return 0;
				case TextureFormat.RG16:
					return 0xFF00;

				case TextureFormat.DXT1:
				case TextureFormat.DXT1Crunched:
				case TextureFormat.DXT3:
				case TextureFormat.DXT5:
				case TextureFormat.DXT5Crunched:
					return 0;

				default:
					throw new NotSupportedException($"Texture format {_this.TextureFormat} isn't supported");
			}
		}

		public static uint DDSBBitMask(this Texture2D _this)
		{
			switch (_this.TextureFormat)
			{
				case TextureFormat.RGBA32:
					return 0x00FF0000;
				case TextureFormat.BGRA32:
					return 0x000000FF;
				case TextureFormat.ARGB32:
					return 0xFF000000;

				case TextureFormat.ARGB4444:
					return 0x000F;
				case TextureFormat.RGBA4444:
					return 0x00F0;

				case TextureFormat.RGB24:
					return 0xFF0000;

				case TextureFormat.RGB565:
					return 0x001F;

				case TextureFormat.Alpha8:
					return 0;

				case TextureFormat.R8:
					return 0;
				case TextureFormat.R16:
					return 0;
				case TextureFormat.RG16:
					return 0;

				case TextureFormat.DXT1:
				case TextureFormat.DXT1Crunched:
				case TextureFormat.DXT3:
				case TextureFormat.DXT5:
				case TextureFormat.DXT5Crunched:
					return 0;

				default:
					throw new NotSupportedException($"Texture format {_this.TextureFormat} isn't supported");
			}
		}

		public static uint DDSABitMask(this Texture2D _this)
		{
			switch (_this.TextureFormat)
			{
				case TextureFormat.RGBA32:
					return 0xFF000000;
				case TextureFormat.BGRA32:
					return 0xFF000000;

				case TextureFormat.ARGB32:
					return 0xFF;
				case TextureFormat.Alpha8:
					return 0xFF;

				case TextureFormat.ARGB4444:
					return 0xF000;
				case TextureFormat.RGBA4444:
					return 0x000F;

				case TextureFormat.RGB24:
					return 0x0;

				case TextureFormat.R8:
					return 0;
				case TextureFormat.R16:
					return 0;
				case TextureFormat.RGB565:
					return 0;
				case TextureFormat.RG16:
					return 0;
				case TextureFormat.DXT1:
				case TextureFormat.DXT1Crunched:
				case TextureFormat.DXT3:
				case TextureFormat.DXT5:
				case TextureFormat.DXT5Crunched:
					return 0;

				default:
					throw new NotSupportedException($"Texture format {_this.TextureFormat} isn't supported");
			}
		}

		public static KTXInternalFormat KTXInternalFormat(this Texture2D _this)
		{
			switch (_this.TextureFormat)
			{
				case TextureFormat.RHalf:
					return KTXFormat.R16F;

				case TextureFormat.RGHalf:
					return KTXFormat.RG16F;

				case TextureFormat.RGBAHalf:
					return KTXFormat.RGBA16F;

				case TextureFormat.RFloat:
					return KTXFormat.R32F;

				case TextureFormat.RGFloat:
					return KTXFormat.RG32F;

				case TextureFormat.RGBAFloat:
					return KTXFormat.RGBA32F;

				case TextureFormat.BC4:
					return KTXFormat.COMPRESSED_RED_RGTC1;

				case TextureFormat.BC5:
					return KTXFormat.COMPRESSED_RG_RGTC2;

				case TextureFormat.BC6H:
					return KTXFormat.COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT;

				case TextureFormat.BC7:
					return KTXFormat.COMPRESSED_RGBA_BPTC_UNORM;

				case TextureFormat.PVRTC_RGB2:
					return KTXFormat.COMPRESSED_RGB_PVRTC_2BPPV1_IMG;

				case TextureFormat.PVRTC_RGBA2:
					return KTXFormat.COMPRESSED_RGBA_PVRTC_2BPPV1_IMG;

				case TextureFormat.PVRTC_RGB4:
					return KTXFormat.COMPRESSED_RGB_PVRTC_4BPPV1_IMG;

				case TextureFormat.PVRTC_RGBA4:
					return KTXFormat.COMPRESSED_RGBA_PVRTC_4BPPV1_IMG;

				case TextureFormat.ETC_RGB4Crunched:
				case TextureFormat.ETC_RGB4_3DS:
				case TextureFormat.ETC_RGB4:
					return KTXFormat.ETC1_RGB8_OES;

				case TextureFormat.ATC_RGB4:
					return KTXFormat.ATC_RGB_AMD;

				case TextureFormat.ATC_RGBA8:
					return KTXFormat.ATC_RGBA_INTERPOLATED_ALPHA_AMD;

				case TextureFormat.EAC_R:
					return KTXFormat.COMPRESSED_R11_EAC;

				case TextureFormat.EAC_R_SIGNED:
					return KTXFormat.COMPRESSED_SIGNED_R11_EAC;

				case TextureFormat.EAC_RG:
					return KTXFormat.COMPRESSED_RG11_EAC;

				case TextureFormat.EAC_RG_SIGNED:
					return KTXFormat.COMPRESSED_SIGNED_RG11_EAC;

				case TextureFormat.ETC2_RGB:
					return KTXFormat.COMPRESSED_RGB8_ETC2;

				case TextureFormat.ETC2_RGBA1:
					return KTXFormat.COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2;

				case TextureFormat.ETC2_RGBA8Crunched:
				case TextureFormat.ETC_RGBA8_3DS:
				case TextureFormat.ETC2_RGBA8:
					return KTXFormat.COMPRESSED_RGBA8_ETC2_EAC;

				default:
					throw new NotSupportedException();
			}
		}

		public static KTXBaseInternalFormat KTXBaseInternalFormat(this Texture2D _this)
		{
			switch (_this.TextureFormat)
			{
				case TextureFormat.RHalf:
				case TextureFormat.RFloat:
				case TextureFormat.BC4:
				case TextureFormat.EAC_R:
				case TextureFormat.EAC_R_SIGNED:
					return KTXBaseFormat.RED;

				case TextureFormat.RGHalf:
				case TextureFormat.RGFloat:
				case TextureFormat.BC5:
				case TextureFormat.EAC_RG:
				case TextureFormat.EAC_RG_SIGNED:
					return KTXBaseFormat.RG;

				case TextureFormat.BC6H:
				case TextureFormat.PVRTC_RGB2:
				case TextureFormat.PVRTC_RGB4:
				case TextureFormat.ETC_RGB4Crunched:
				case TextureFormat.ETC_RGB4_3DS:
				case TextureFormat.ETC_RGB4:
				case TextureFormat.ATC_RGB4:
				case TextureFormat.ETC2_RGB:
					return KTXBaseFormat.RGB;

				case TextureFormat.RGBAHalf:
				case TextureFormat.RGBAFloat:
				case TextureFormat.BC7:
				case TextureFormat.PVRTC_RGBA2:
				case TextureFormat.PVRTC_RGBA4:
				case TextureFormat.ATC_RGBA8:
				case TextureFormat.ETC2_RGBA8Crunched:
				case TextureFormat.ETC_RGBA8_3DS:
				case TextureFormat.ETC2_RGBA8:
				case TextureFormat.ETC2_RGBA1:
					return KTXBaseFormat.RGBA;

				default:
					throw new NotSupportedException();
			}
		}

		public static PVRPixelFormat PVRPixelFormat(this Texture2D _this)
		{
			switch (_this.TextureFormat)
			{
				case TextureFormat.YUY2:
					return PVRFormat.YUY2;

				case TextureFormat.PVRTC_RGB2:
					return PVRFormat.PVRTC2bppRGB;

				case TextureFormat.PVRTC_RGBA2:
					return PVRFormat.PVRTC2bppRGBA;

				case TextureFormat.PVRTC_RGB4:
					return PVRFormat.PVRTC4bppRGB;

				case TextureFormat.PVRTC_RGBA4:
					return PVRFormat.PVRTC4bppRGBA;

				case TextureFormat.ETC_RGB4Crunched:
				case TextureFormat.ETC_RGB4_3DS:
				case TextureFormat.ETC_RGB4:
					return PVRFormat.ETC1;

				case TextureFormat.ETC2_RGB:
					return PVRFormat.ETC2RGB;

				case TextureFormat.ETC2_RGBA1:
					return PVRFormat.ETC2RGBA1;

				case TextureFormat.ETC2_RGBA8Crunched:
				case TextureFormat.ETC_RGBA8_3DS:
				case TextureFormat.ETC2_RGBA8:
					return PVRFormat.ETC2RGBA;

				case TextureFormat.ASTC_RGB_4x4:
				case TextureFormat.ASTC_RGBA_4x4:
					return PVRFormat.ASTC_4x4;

				case TextureFormat.ASTC_RGB_5x5:
				case TextureFormat.ASTC_RGBA_5x5:
					return PVRFormat.ASTC_5x5;

				case TextureFormat.ASTC_RGB_6x6:
				case TextureFormat.ASTC_RGBA_6x6:
					return PVRFormat.ASTC_6x6;

				case TextureFormat.ASTC_RGB_8x8:
				case TextureFormat.ASTC_RGBA_8x8:
					return PVRFormat.ASTC_8x8;

				case TextureFormat.ASTC_RGB_10x10:
				case TextureFormat.ASTC_RGBA_10x10:
					return PVRFormat.ASTC_10x10;

				case TextureFormat.ASTC_RGB_12x12:
				case TextureFormat.ASTC_RGBA_12x12:
					return PVRFormat.ASTC_12x12;

				default:
					throw new NotSupportedException();
			}
		}

		public static int PVRTCBitCount(this Texture2D _this)
		{
			switch (_this.TextureFormat)
			{
				case TextureFormat.PVRTC_RGB2:
				case TextureFormat.PVRTC_RGBA2:
					return 2;

				case TextureFormat.PVRTC_RGB4:
				case TextureFormat.PVRTC_RGBA4:
					return 4;

				default:
					throw new NotSupportedException(_this.TextureFormat.ToString());
			}
		}

		public static int ASTCBlockSize(this Texture2D _this)
		{
			switch (_this.TextureFormat)
			{
				case TextureFormat.ASTC_RGB_4x4:
				case TextureFormat.ASTC_RGBA_4x4:
					return 4;

				case TextureFormat.ASTC_RGB_5x5:
				case TextureFormat.ASTC_RGBA_5x5:
					return 5;

				case TextureFormat.ASTC_RGB_6x6:
				case TextureFormat.ASTC_RGBA_6x6:
					return 6;

				case TextureFormat.ASTC_RGB_8x8:
				case TextureFormat.ASTC_RGBA_8x8:
					return 8;

				case TextureFormat.ASTC_RGB_10x10:
				case TextureFormat.ASTC_RGBA_10x10:
					return 10;

				case TextureFormat.ASTC_RGB_12x12:
				case TextureFormat.ASTC_RGBA_12x12:
					return 12;

				default:
					throw new NotSupportedException(_this.TextureFormat.ToString());
			}
		}
	}
}
