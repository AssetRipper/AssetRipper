using AssetRipper.Core.Logging;
using AssetRipper.Library.Utils;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.TextureDecoder.Astc;
using AssetRipper.TextureDecoder.Atc;
using AssetRipper.TextureDecoder.Bc;
using AssetRipper.TextureDecoder.Dxt;
using AssetRipper.TextureDecoder.Etc;
using AssetRipper.TextureDecoder.Pvrtc;
using AssetRipper.TextureDecoder.Rgb;
using AssetRipper.TextureDecoder.Yuy2;


namespace AssetRipper.Library.Exporters.Textures
{
	public static class TextureConverter
	{
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
			byte[] decompressed = CrunchHandler.DecompressCrunch(textureFormat, width, height, unityVersion, data);
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
					case TextureFormat.BGRA32_14:
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
			byte[] decompressed = CrunchHandler.DecompressCrunch(textureFormat, width, height, unityVersion, data);
			return ETCTextureToBitmap(textureFormat, width, height, decompressed);
		}

		public static DirectBitmap ATCTextureToBitmap(TextureFormat textureFormat, int width, int height, byte[] data)
		{
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				switch (textureFormat)
				{
					case TextureFormat.ATC_RGB4_35:
						AtcDecoder.DecompressAtcRgb4(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.ATC_RGBA8_36:
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
				PvrtcDecoder.DecompressPVRTC(data, width, height, bitCount == 2, bitmap.Bits);
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

		public static DirectBitmap BcTextureToBitmap(TextureFormat textureFormat, int width, int height, byte[] data)
		{
			Logger.Info(LogCategory.Export, "Uses Bc Decoding!");
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				DecodeBC(data, textureFormat, width, height, bitmap.Bits);
				bitmap.FlipY();
				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
		}

		private static byte[] DecodeBC(byte[] inputData, TextureFormat textureFormat, int width, int height, byte[] outputData)
		{
			byte[] result = new byte[4 * width * height];
			if (width % 4 != 0 || height % 4 != 0) //Managed code doesn't currently handle partial block sizes well.
			{
				NativeDecodeBC(inputData, textureFormat, width, height, result);
			}
			else
			{
				ManagedDecodeBC(inputData, textureFormat, width, height, result);
			}
			return result;
		}

		private static bool NativeDecodeBC(byte[] inputData, TextureFormat textureFormat, int width, int height, byte[] outputData)
		{
			Logger.Info(LogCategory.Export, $"Performing alternate decoding for {textureFormat}");

			switch (textureFormat)
			{
				case TextureFormat.BC4:
					Texture2DDecoder.TextureDecoder.DecodeBC4(inputData, width, height, outputData);
					return true;
				case TextureFormat.BC5:
					Texture2DDecoder.TextureDecoder.DecodeBC5(inputData, width, height, outputData);
					return true;
				case TextureFormat.BC6H:
					Texture2DDecoder.TextureDecoder.DecodeBC6(inputData, width, height, outputData);
					return true;
				case TextureFormat.BC7:
					Texture2DDecoder.TextureDecoder.DecodeBC7(inputData, width, height, outputData);
					return true;
				default:
					return false;
			}
		}

		private static bool ManagedDecodeBC(byte[] inputData, TextureFormat textureFormat, int width, int height, byte[] outputData)
		{
			switch (textureFormat)
			{
				case TextureFormat.BC4:
					BcDecoder.DecompressBC4(inputData, width, height, outputData);
					return true;
				case TextureFormat.BC5:
					BcDecoder.DecompressBC5(inputData, width, height, outputData);
					return true;
				case TextureFormat.BC6H:
					BcDecoder.DecompressBC6H(inputData, width, height, false, outputData);
					return true;
				case TextureFormat.BC7:
					BcDecoder.DecompressBC7(inputData, width, height, outputData);
					return true;
				default:
					return false;
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
				double vr = (r * 2.0) - 255.0;
				double vg = (g * 2.0) - 255.0;
				double hypotenuseSqr = (vr * vr) + (vg * vg);
				hypotenuseSqr = hypotenuseSqr > MagnitudeSqr ? MagnitudeSqr : hypotenuseSqr;
				double b = (Math.Sqrt(MagnitudeSqr - hypotenuseSqr) + 255.0) / 2.0;
				dataPtr[0] = (byte)b;
			}
		}
	}
}
