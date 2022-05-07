using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Library.Exporters.Textures.Enums;
using System;

namespace AssetRipper.Library.Exporters.Textures.Extensions
{
	public static class Texture2DExtensions
	{
		/// <summary>
		/// Get the applicable KTX base format for a texture
		/// </summary>
		/// <param name="_this">The texture</param>
		/// <param name="safe">If true, this method will return RG rather than throwing an exception.</param>
		/// <returns>The format</returns>
		/// <exception cref="NotSupportedException">if not safe and not an pvrtc texture</exception>
		public static KTXBaseInternalFormat GetKTXBaseInternalFormat(this ITexture2D _this, bool safe)
		{
			switch (_this.TextureFormat)
			{
				case TextureFormat.RHalf:
				case TextureFormat.RFloat:
				case TextureFormat.BC4:
				case TextureFormat.EAC_R:
				case TextureFormat.EAC_R_SIGNED:
					return KTXBaseInternalFormat.RED;

				case TextureFormat.RGHalf:
				case TextureFormat.RGFloat:
				case TextureFormat.BC5:
				case TextureFormat.EAC_RG:
				case TextureFormat.EAC_RG_SIGNED:
					return KTXBaseInternalFormat.RG;

				case TextureFormat.BC6H:
				case TextureFormat.PVRTC_RGB2:
				case TextureFormat.PVRTC_RGB4:
				case TextureFormat.ETC_RGB4Crunched:
				case TextureFormat.ETC_RGB4_3DS:
				case TextureFormat.ETC_RGB4:
				case TextureFormat.ATC_RGB4:
				case TextureFormat.ETC2_RGB:
					return KTXBaseInternalFormat.RGB;

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
					return KTXBaseInternalFormat.RGBA;

				default:
					{
						if (safe)
							return KTXBaseInternalFormat.RG;
						else
							throw new NotSupportedException(_this.TextureFormat.ToString());
					}
			}
		}

		/// <summary>
		/// Get the cooresponding PVRTC bit count for a texture
		/// </summary>
		/// <param name="_this">The texture</param>
		/// <param name="safe">If true, this method will return zero rather than throwing an exception.</param>
		/// <returns>The bit count</returns>
		/// <exception cref="NotSupportedException">if not safe and not an pvrtc texture</exception>
		public static int PVRTCBitCount(this ITexture2D _this, bool safe)
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
					{
						if (safe)
							return 0;
						else
							throw new NotSupportedException(_this.TextureFormat.ToString());
					}
			}
		}

		/// <summary>
		/// Get the cooresponding ASTC block size for a texture
		/// </summary>
		/// <param name="_this">The texture</param>
		/// <param name="safe">If true, this method will return zero rather than throwing an exception.</param>
		/// <returns>The length of one side of the block</returns>
		/// <exception cref="NotSupportedException">if not safe and not an astc texture</exception>
		public static int ASTCBlockSize(this ITexture2D _this, bool safe)
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
					{
						if (safe)
							return 0;
						else
							throw new NotSupportedException(_this.TextureFormat.ToString());
					}
			}
		}
	}
}
