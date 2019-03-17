using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using uTinyRipper;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Textures;
using uTinyRipperGUI.TextureContainers.DDS;
using uTinyRipperGUI.TextureContainers.KTX;
using uTinyRipperGUI.TextureContainers.PVR;
using uTinyRipperGUI.TextureConverters;
using Version = uTinyRipper.Version;

namespace uTinyRipperGUI.Exporters
{
	public static class TextureConverter
	{
#warning TODO: replace to  other libs
		[DllImport("PVRTexLibWrapper", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool DecompressPVR(byte[] buffer, IntPtr bmp, int len);

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


		public static Bitmap DDSTextureToBitmap(Texture2D texture, byte[] data)
		{
			DDSConvertParameters @params = new DDSConvertParameters()
			{
				DataLength = data.LongLength,
				MipMapCount = texture.MipCount,
				Width = texture.Width,
				Height = texture.Height,
				IsPitchOrLinearSize = texture.DDSIsPitchOrLinearSize(),
				PixelFormatFlags = texture.DDSPixelFormatFlags(),
				FourCC = (DDSFourCCType)texture.DDSFourCC(),
				RGBBitCount = texture.DDSRGBBitCount(),
				RBitMask = texture.DDSRBitMask(),
				GBitMask = texture.DDSGBitMask(),
				BBitMask = texture.DDSBBitMask(),
				ABitMask = texture.DDSABitMask(),
				Caps = texture.DDSCaps(),
			};

			int width = @params.Width;
			int height = @params.Height;
			int size = width * height * 4;
			byte[] buffer = new byte[size];
			using (MemoryStream destination = new MemoryStream(buffer))
			{
				using (MemoryStream source = new MemoryStream(data))
				{
					EndianType endianess = Texture2D.IsSwapBytes(texture.File.Platform, texture.TextureFormat) ? EndianType.BigEndian : EndianType.LittleEndian;
					using (EndianReader sourceReader = new EndianReader(source, endianess))
					{
						DecompressDDS(sourceReader, destination, @params);
					}
				}

				Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				Rectangle rect = new Rectangle(0, 0, width, height);
				BitmapData bitData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
				IntPtr pointer = bitData.Scan0;
				Marshal.Copy(buffer, 0, pointer, size);
				bitmap.UnlockBits(bitData);
				return bitmap;
			}
		}

		public static Bitmap DDSCrunchedTextureToBitmap(Texture2D texture, byte[] data)
		{
			byte[] decompressed = DecompressCrunch(texture, data);
			return DDSTextureToBitmap(texture, decompressed);
		}

		public static Bitmap PVRTextureToBitmap(Texture2D texture, byte[] data)
		{
			using (MemoryStream dstStream = new MemoryStream())
			{
				PVRConvertParameters @params = new PVRConvertParameters
				{
					DataLength = data.Length,
					PixelFormat = texture.PVRPixelFormat(),
					Width = texture.Width,
					Height = texture.Height,
					MipMapCount = texture.MipCount,
				};
				using (MemoryStream srcStream = new MemoryStream(data))
				{
					PVRConverter.ExportPVR(dstStream, srcStream, @params);
				}
				return PVRToBitmap(dstStream.ToArray(), texture.Width, texture.Height);
			}
		}

		public static Bitmap PVRCrunchedTextureToBitmap(Texture2D texture, byte[] data)
		{
			byte[] decompressed = DecompressCrunch(texture, data);
			using (MemoryStream dstStream = new MemoryStream())
			{
				using (MemoryStream srcStream = new MemoryStream(decompressed))
				{
					PVRConvertParameters @params = new PVRConvertParameters
					{
						DataLength = decompressed.Length,
						PixelFormat = texture.PVRPixelFormat(),
						Width = texture.Width,
						Height = texture.Height,
						MipMapCount = texture.MipCount,
					};
					PVRConverter.ExportPVR(dstStream, srcStream, @params);
				}
				return PVRTextureToBitmap(texture, dstStream.ToArray());
			}
		}

		public static Bitmap TextureConverterTextureToBitmap(Texture2D texture, byte[] data)
		{
			Bitmap bitmap = null;
			BitmapData bmd = null;
			try
			{
				bitmap = new Bitmap(texture.Width, texture.Height);
				Rectangle rect = new Rectangle(0, 0, texture.Width, texture.Height);
				bmd = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				int len = Math.Abs(bmd.Stride) * bmd.Height;
				bool fixAlpha = texture.KTXBaseInternalFormat() == KTXBaseInternalFormat.RED || texture.KTXBaseInternalFormat() == KTXBaseInternalFormat.RG;
				if (!Ponvert(data, bmd.Scan0, texture.Width, texture.Height, data.Length, (int)ToQFormat(texture.TextureFormat), len, fixAlpha))
				{
					bitmap.UnlockBits(bmd);
					bitmap.Dispose();
					return null;
				}

				bitmap.UnlockBits(bmd);
				bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
				return bitmap;
			}
			catch
			{
				if (bitmap != null)
				{
					if (bmd != null)
					{
						bitmap.UnlockBits(bmd);
					}
					bitmap.Dispose();
				}

				throw;
			}
		}

		public static Bitmap TexgenpackTextureToBitmap(Texture2D texture, byte[] data)
		{
			Bitmap bitmap = null;
			BitmapData bmd = null;
			try
			{
				bitmap = new Bitmap(texture.Width, texture.Height);
				Rectangle rect = new Rectangle(0, 0, texture.Width, texture.Height);
				bmd = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				bool fixAlpha = texture.KTXBaseInternalFormat() == KTXBaseInternalFormat.RED || texture.KTXBaseInternalFormat() == KTXBaseInternalFormat.RG;
				texgenpackdecode((int)ToTexgenpackTexturetype(texture.TextureFormat), data, texture.Width, texture.Height, bmd.Scan0, fixAlpha);
				bitmap.UnlockBits(bmd);
				bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
				return bitmap;
			}
			catch
			{
				if (bitmap != null)
				{
					if (bmd != null)
					{
						bitmap.UnlockBits(bmd);
					}
					bitmap.Dispose();
				}

				throw;
			}
		}

		public static Bitmap PVRToBitmap(byte[] data, int width, int height)
		{
			Bitmap bitmap = null;
			BitmapData bmd = null;
			try
			{
				bitmap = new Bitmap(width, height);
				Rectangle rect = new Rectangle(0, 0, width, height);
				bmd = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				int len = Math.Abs(bmd.Stride) * bmd.Height;
				if (!DecompressPVR(data, bmd.Scan0, len))
				{
					bitmap.UnlockBits(bmd);
					bitmap.Dispose();
					return null;
				}

				bitmap.UnlockBits(bmd);
				bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
				return bitmap;
			}
			catch
			{
				if (bitmap != null)
				{
					if (bmd != null)
					{
						bitmap.UnlockBits(bmd);
					}
					bitmap.Dispose();
				}

				throw;
			}
		}

		private static void DecompressDDS(BinaryReader reader, Stream destination, DDSConvertParameters @params)
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
