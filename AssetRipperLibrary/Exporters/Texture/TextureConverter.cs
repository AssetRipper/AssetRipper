using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Logging;
using AssetRipper.Library.TextureContainers.KTX;
using AssetRipper.Library.TextureDecoders.Astc;
using AssetRipper.Library.TextureDecoders.Atc;
using AssetRipper.Library.TextureDecoders.Dxt;
using AssetRipper.Library.TextureDecoders.Etc;
using AssetRipper.Library.TextureDecoders.Pvrtc;
using AssetRipper.Library.TextureDecoders.Rgb;
using AssetRipper.Library.TextureDecoders.Yuy2;
using AssetRipper.Library.Utils;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Texture2DDecoder;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Library.Exporters.Textures
{
	[SupportedOSPlatform("windows")]
	public static class TextureConverter
	{
		[DllImport("texgenpack", CallingConvention = CallingConvention.Cdecl)]
		private static extern void texgenpackdecode(int texturetype, byte[] texturedata, int width, int height, IntPtr bmp, bool fixAlpha);

		private static bool IsUseUnityCrunch(UnityVersion version, TextureFormat format)
		{
			if (version.IsGreaterEqual(2017, 3))
			{
				return true;
			}
			return format == TextureFormat.ETC_RGB4Crunched || format == TextureFormat.ETC2_RGBA8Crunched;
		}

		public static DirectBitmap DXTTextureToBitmap(TextureFormat textureFormat, int width, int height, byte[] data)
		{
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				switch (textureFormat)
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
						throw new Exception(textureFormat.ToString());

				}
				bitmap.FlipY();
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		public static DirectBitmap DXTCrunchedTextureToBitmap(TextureFormat textureFormat, int width, int height, UnityVersion unityVersion, byte[] data)
		{
			byte[] decompressed = DecompressCrunch(textureFormat, width, height, unityVersion, data);
			return DXTTextureToBitmap(textureFormat, width, height, decompressed);
		}

		public static DirectBitmap RGBTextureToBitmap(TextureFormat textureFormat, int width, int height, byte[] data)
		{
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				switch (textureFormat)
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
					case TextureFormat.RHalf:
						RgbConverter.RHalfToBGRA32(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.RGHalf:
						RgbConverter.RGHalfToBGRA32(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.RGBAHalf:
						RgbConverter.RGBAHalfToBGRA32(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.RFloat:
						RgbConverter.RFloatToBGRA32(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.RGFloat:
						RgbConverter.RGFloatToBGRA32(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.RGBAFloat:
						RgbConverter.RGBAFloatToBGRA32(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.RGB9e5Float:
						RgbConverter.RGB9e5FloatToBGRA32(data, width, height, bitmap.Bits);
						break;

					default:
						throw new Exception(textureFormat.ToString());

				}
				bitmap.FlipY();
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		public static DirectBitmap ETCTextureToBitmap(TextureFormat textureFormat, int width, int height, byte[] data)
		{
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				switch (textureFormat)
				{
					case TextureFormat.ETC_RGB4:
					case TextureFormat.ETC_RGB4_3DS:
					case TextureFormat.ETC_RGB4Crunched:
						EtcDecoder.DecompressETC(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.EAC_R:
						EtcDecoder.DecompressEACRUnsigned(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.EAC_R_SIGNED:
						EtcDecoder.DecompressEACRSigned(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.EAC_RG:
						EtcDecoder.DecompressEACRGUnsigned(data, width, height, bitmap.Bits);
						break;
					case TextureFormat.EAC_RG_SIGNED:
						EtcDecoder.DecompressEACRGSigned(data, width, height, bitmap.Bits);
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
						throw new Exception(textureFormat.ToString());

				}
				bitmap.FlipY();
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		public static DirectBitmap ETCCrunchedTextureToBitmap(TextureFormat textureFormat, int width, int height, UnityVersion unityVersion, byte[] data)
		{
			byte[] decompressed = DecompressCrunch(textureFormat, width, height, unityVersion, data);
			return ETCTextureToBitmap(textureFormat, width, height, decompressed);
		}

		public static DirectBitmap ATCTextureToBitmap(TextureFormat textureFormat, int width, int height, byte[] data)
		{
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				switch (textureFormat)
				{
					case TextureFormat.ATC_RGB4:
						AtcDecoder.DecompressAtcRgb4(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.ATC_RGBA8:
						AtcDecoder.DecompressAtcRgba8(data, width, height, bitmap.Bits);
						break;

					default:
						throw new Exception(textureFormat.ToString());

				}
				bitmap.FlipY();
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		public static DirectBitmap YUY2TextureToBitmap(TextureFormat textureFormat, int width, int height, byte[] data)
		{
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

		public static DirectBitmap PVRTCTextureToBitmap(int bitCount, TextureFormat textureFormat, int width, int height, byte[] data)
		{
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				PvrtcDecoder.DecompressPVRTC(data, width, height, bitmap.Bits, bitCount == 2);
				bitmap.FlipY();
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		public static DirectBitmap ASTCTextureToBitmap(int blockSize, int width, int height, byte[] data)
		{
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				AstcDecoder.DecodeASTC(data, width, height, blockSize, blockSize, bitmap.Bits);
				bitmap.FlipY();
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		public static DirectBitmap TexgenpackTextureToBitmap(KTXBaseInternalFormat baseInternalFormat, TextureFormat textureFormat, int width, int height, byte[] data)
		{
			Logger.Info("Uses texgenpack!");
			bool fixAlpha = baseInternalFormat is KTXBaseInternalFormat.RED or KTXBaseInternalFormat.RG;
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				texgenpackdecode((int)ToTexgenpackTexturetype(textureFormat), data, width, height, bitmap.BitsPtr, fixAlpha);
				bitmap.FlipY();
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		public unsafe static void UnpackNormal(IntPtr inputOutput, int length)
		{
			byte* dataPtr = (byte*)inputOutput;
			int count = length / 4;
			for (int i = 0; i < count; i++, dataPtr += 4)
			{
				byte r = dataPtr[3];
				byte g = dataPtr[1];
				byte a = dataPtr[2];
				dataPtr[2] = r;
				dataPtr[3] = a;

				const double MagnitudeSqr = 255.0 * 255.0;
				double vr = r * 2.0 - 255.0;
				double vg = g * 2.0 - 255.0;
				double hypotenuseSqr = vr * vr + vg * vg;
				hypotenuseSqr = hypotenuseSqr > MagnitudeSqr ? MagnitudeSqr : hypotenuseSqr;
				double b = (System.Math.Sqrt(MagnitudeSqr - hypotenuseSqr) + 255.0) / 2.0;
				dataPtr[0] = (byte)b;
			}
		}

		private static byte[] DecompressCrunch(TextureFormat textureFormat, int width, int height, UnityVersion unityVersion, byte[] data)
		{
			bool result = IsUseUnityCrunch(unityVersion, textureFormat) ?
					DecompressUnityCRN(data, out byte[] uncompressedBytes) :
					DecompressCRN(data, out uncompressedBytes);
			if (result)
			{
				return uncompressedBytes;
			}
			else
			{
				throw new Exception("Unable to decompress crunched texture");
			}
		}

		private static bool DecompressCRN(byte[] data, out byte[] uncompressedBytes)
		{
			uncompressedBytes = TextureDecoder.UnpackCrunch(data);
			return uncompressedBytes != null;
		}

		private static bool DecompressUnityCRN(byte[] data, out byte[] uncompressedBytes)
		{
			uncompressedBytes = TextureDecoder.UnpackUnityCrunch(data);
			return uncompressedBytes != null;
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
