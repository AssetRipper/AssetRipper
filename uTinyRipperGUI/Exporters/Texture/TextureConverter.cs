using Astc;
using Dxt;
using Etc;
using Pvrtc;
using Rgb;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using uTinyRipper;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Textures;
using uTinyRipperGUI.TextureContainers.DDS;
using uTinyRipperGUI.TextureContainers.KTX;
using uTinyRipperGUI.TextureContainers.PVR;
using uTinyRipperGUI.TextureConverters;
using Yuy2;
using Version = uTinyRipper.Version;

namespace uTinyRipperGUI.Exporters
{
	public static class TextureConverter
	{
		[DllImport("TextureConverterWrapper", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool Ponvert(byte[] buffer, IntPtr bmp, int nWidth, int nHeight, int len, int type, int bmpsize, bool fixAlpha);

		[DllImport("texgenpack", CallingConvention = CallingConvention.Cdecl)]
		private static extern void texgenpackdecode(int texturetype, byte[] texturedata, int width, int height, IntPtr bmp, bool fixAlpha);

		[DllImport("crunch", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool DecompressCRN(byte[] pSrcFileData, int srcFileSize, out IntPtr uncompressedData, out int uncompressedSize);

		[DllImport("crunchunity.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool DecompressUnityCRN(byte[] pSrc_file_data, int src_file_size, out IntPtr uncompressedData, out int uncompressedSize);

		private static bool IsUseUnityCrunch(Version version, TextureFormat format)
		{
			if (version.IsGreaterEqual(2017, 3))
			{
				return true;
			}
			return format == TextureFormat.ETC_RGB4Crunched || format == TextureFormat.ETC2_RGBA8Crunched;
		}

		public static DirectBitmap DXTTextureToBitmap(Texture2D texture, byte[] data)
		{
			int width = texture.Width;
			int height = texture.Height;
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				switch (texture.TextureFormat)
				{
					case TextureFormat.DXT1:
					case TextureFormat.DXT1Crunched:
						DxtDecoder.DecompressDXT1(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.DXT3:
						DxtDecoder.DecompressDXT3(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.DXT5:
					case TextureFormat.DXT5Crunched:
						DxtDecoder.DecompressDXT5(data, width, height, bitmap.Bits);
						break;

					default:
						throw new Exception(texture.TextureFormat.ToString());

				}
				bitmap.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		public static DirectBitmap DXTCrunchedTextureToBitmap(Texture2D texture, byte[] data)
		{
			byte[] decompressed = DecompressCrunch(texture, data);
			return DXTTextureToBitmap(texture, decompressed);
		}

		public static DirectBitmap RGBTextureToBitmap(Texture2D texture, byte[] data)
		{
			int width = texture.Width;
			int height = texture.Height;
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				switch (texture.TextureFormat)
				{
					case TextureFormat.Alpha8:
						RgbConverter.A8ToBGRA32(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.ARGB4444:
						RgbConverter.ARGB16ToBGRA32(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.RGB24:
						RgbConverter.RGB24ToBGRA32(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.RGBA32:
						RgbConverter.RGBA32ToBGRA32(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.ARGB32:
						RgbConverter.ARGB32ToBGRA32(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.RGB565:
						RgbConverter.RGB16ToBGRA32(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.R16:
						RgbConverter.R16ToBGRA32(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.RGBA4444:
						RgbConverter.RGBA16ToBGRA32(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.BGRA32:
						Buffer.BlockCopy(data, 0, bitmap.Bits, 0, bitmap.Bits.Length);
						break;
					case TextureFormat.RG16:
						RgbConverter.RG16ToBGRA32(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.R8:
						RgbConverter.R8ToBGRA32(data, width, height, bitmap.Bits);
						break;

					default:
						throw new Exception(texture.TextureFormat.ToString());

				}
				bitmap.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		public static DirectBitmap ETCTextureToBitmap(Texture2D texture, byte[] data)
		{
			int width = texture.Width;
			int height = texture.Height;
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				switch (texture.TextureFormat)
				{
					case TextureFormat.ETC_RGB4:
					case TextureFormat.ETC_RGB4_3DS:
					case TextureFormat.ETC_RGB4Crunched:
						EtcDecoder.DecompressETC(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.ETC2_RGB:
						EtcDecoder.DecompressETC2(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.ETC2_RGBA1:
						EtcDecoder.DecompressETC2A1(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.ETC2_RGBA8:
					case TextureFormat.ETC_RGBA8_3DS:
					case TextureFormat.ETC2_RGBA8Crunched:
						EtcDecoder.DecompressETC2A8(data, width, height, bitmap.Bits);
						break;

					default:
						throw new Exception(texture.TextureFormat.ToString());

				}
				bitmap.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		public static DirectBitmap ETCCrunchedTextureToBitmap(Texture2D texture, byte[] data)
		{
			byte[] decompressed = DecompressCrunch(texture, data);
			return ETCTextureToBitmap(texture, decompressed);
		}

		public static DirectBitmap YUY2TextureToBitmap(Texture2D texture, byte[] data)
		{
			int width = texture.Width;
			int height = texture.Height;
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				Yuy2Decoder.DecompressYUY2(data, width, height, bitmap.Bits);
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		public static DirectBitmap PVRTCTextureToBitmap(Texture2D texture, byte[] data)
		{
			int width = texture.Width;
			int height = texture.Height;
			int bitCount = texture.PVRTCBitCount();
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				PvrtcDecoder.DecompressPVRTC(data, width, height, bitmap.Bits, bitCount == 2);
				bitmap.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		public static DirectBitmap ASTCTextureToBitmap(Texture2D texture, byte[] data)
		{
			int width = texture.Width;
			int height = texture.Height;
			int blockSize = texture.ASTCBlockSize();
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				AstcDecoder.DecodeASTC(data, width, height, blockSize, blockSize, bitmap.Bits);
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		public static DirectBitmap TextureConverterTextureToBitmap(Texture2D texture, byte[] data)
		{
			bool fixAlpha = texture.KTXBaseInternalFormat() == KTXBaseInternalFormat.RED || texture.KTXBaseInternalFormat() == KTXBaseInternalFormat.RG;
			DirectBitmap bitmap = new DirectBitmap(texture.Width, texture.Height);
			try
			{
				int len = bitmap.Stride * bitmap.Height;
				if (Ponvert(data, bitmap.BitsPtr, texture.Width, texture.Height, data.Length, (int)ToQFormat(texture.TextureFormat), len, fixAlpha))
				{
					bitmap.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
					return bitmap;
				}
				else
				{
					bitmap.Dispose();
					return null;
				}
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		public static DirectBitmap TexgenpackTextureToBitmap(Texture2D texture, byte[] data)
		{
			bool fixAlpha = texture.KTXBaseInternalFormat() == KTXBaseInternalFormat.RED || texture.KTXBaseInternalFormat() == KTXBaseInternalFormat.RG;
			DirectBitmap bitmap = new DirectBitmap(texture.Width, texture.Height);
			try
			{
				texgenpackdecode((int)ToTexgenpackTexturetype(texture.TextureFormat), data, texture.Width, texture.Height, bitmap.BitsPtr, fixAlpha);
				bitmap.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		private static void DecompressDDS(BinaryReader reader, Stream destination, DDSContainerParameters @params)
		{
			if (@params.PixelFormatFlags.IsFourCC())
			{
				switch (@params.FourCC)
				{
					case DDSFourCCType.DXT1:
						DDSDecompressor.DecompressDXT1(reader, destination, @params);
						break;
					case DDSFourCCType.DXT3:
						DDSDecompressor.DecompressDXT3(reader, destination, @params);
						break;
					case DDSFourCCType.DXT5:
						DDSDecompressor.DecompressDXT5(reader, destination, @params);
						break;

					default:
						throw new NotImplementedException(@params.FourCC.ToString());
				}
			}
			else
			{
				if (@params.PixelFormatFlags.IsLuminace())
				{
					throw new NotSupportedException("Luminace isn't supported");
				}
				else
				{
					if (@params.PixelFormatFlags.IsAlphaPixels())
					{
						DDSDecompressor.DecompressRGBA(reader, destination, @params);
					}
					else
					{
						DDSDecompressor.DecompressRGB(reader, destination, @params);
					}
				}
			}
		}

		private static byte[] DecompressCrunch(Texture2D texture, byte[] data)
		{
			IntPtr uncompressedData = default;
			try
			{
				bool result = IsUseUnityCrunch(texture.File.Version, texture.TextureFormat) ?
					DecompressUnityCRN(data, data.Length, out uncompressedData, out int uncompressedSize) :
					DecompressCRN(data, data.Length, out uncompressedData, out uncompressedSize);
				if (result)
				{
					byte[] uncompressedBytes = new byte[uncompressedSize];
					Marshal.Copy(uncompressedData, uncompressedBytes, 0, uncompressedSize);
					return uncompressedBytes;
				}
				else
				{
					throw new Exception("Unable to decompress crunched texture");
				}
			}
			finally
			{
				Marshal.FreeHGlobal(uncompressedData);
			}
		}

		private static QFormat ToQFormat(TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.DXT1:
				case TextureFormat.DXT1Crunched:
					return QFormat.Q_FORMAT_S3TC_DXT1_RGB;

				case TextureFormat.DXT3:
					return QFormat.Q_FORMAT_S3TC_DXT3_RGBA;

				case TextureFormat.DXT5:
				case TextureFormat.DXT5Crunched:
					return QFormat.Q_FORMAT_S3TC_DXT5_RGBA;

				case TextureFormat.RHalf:
					return QFormat.Q_FORMAT_R_16F;

				case TextureFormat.RGHalf:
					return QFormat.Q_FORMAT_RG_HF;

				case TextureFormat.RGBAHalf:
					return QFormat.Q_FORMAT_RGBA_HF;

				case TextureFormat.RFloat:
					return QFormat.Q_FORMAT_R_F;

				case TextureFormat.RGFloat:
					return QFormat.Q_FORMAT_RG_F;

				case TextureFormat.RGBAFloat:
					return QFormat.Q_FORMAT_RGBA_F;

				case TextureFormat.RGB9e5Float:
					return QFormat.Q_FORMAT_RGB9_E5;

				case TextureFormat.ATC_RGB4:
					return QFormat.Q_FORMAT_ATITC_RGB;

				case TextureFormat.ATC_RGBA8:
					return QFormat.Q_FORMAT_ATC_RGBA_INTERPOLATED_ALPHA;

				case TextureFormat.EAC_R:
					return QFormat.Q_FORMAT_EAC_R_UNSIGNED;

				case TextureFormat.EAC_R_SIGNED:
					return QFormat.Q_FORMAT_EAC_R_SIGNED;

				case TextureFormat.EAC_RG:
					return QFormat.Q_FORMAT_EAC_RG_UNSIGNED;

				case TextureFormat.EAC_RG_SIGNED:
					return QFormat.Q_FORMAT_EAC_RG_SIGNED;

				default:
					throw new NotSupportedException(format.ToString());
			}
		}

		private static TexgenpackTexturetype ToTexgenpackTexturetype(TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.BC4:
					return TexgenpackTexturetype.RGTC1;

				case TextureFormat.BC5:
					return TexgenpackTexturetype.RGTC2;

				case TextureFormat.BC6H:
					return TexgenpackTexturetype.BPTC_FLOAT;

				case TextureFormat.BC7:
					return TexgenpackTexturetype.BPTC;

				default:
					throw new NotSupportedException(format.ToString());
			}
		}
	}
}
