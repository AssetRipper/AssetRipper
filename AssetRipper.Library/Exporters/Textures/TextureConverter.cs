using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Logging;
using AssetRipper.Library.Exporters.Textures.Enums;
using AssetRipper.Library.Utils;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.TextureDecoder.Astc;
using AssetRipper.TextureDecoder.Atc;
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
			Logger.Verbose("Uses texgenpack!");
			Logger.Verbose($"KTXBaseInternalFormat: {baseInternalFormat}");
			bool fixAlpha = baseInternalFormat is KTXBaseInternalFormat.RED or KTXBaseInternalFormat.RG;
			Logger.Verbose($"Fix alpha: {fixAlpha}");
			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				if (TexGenPackHandler.Decode(textureFormat, data, width, height, bitmap.BitsPtr, fixAlpha))
				{
					Logger.Verbose($"Byte array length: {bitmap.Bits.Length} Width: {width} Height: {height}");
					CheckEqual(DecodeBC(data, textureFormat, width, height), bitmap.Bits);
				}
				else
				{
					DecodeBC(data, textureFormat, width, height, bitmap.Bits);
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

		private static byte[] DecodeBC(byte[] inputData, TextureFormat textureFormat, int width, int height)
		{
			byte[] result = new byte[4 * width * height];
			DecodeBC(inputData, textureFormat, width, height, result);
			return result;
		}

		private static bool DecodeBC(byte[] inputData, TextureFormat textureFormat, int width, int height, byte[] outputData)
		{
			Logger.Verbose($"Performing alternate decoding for {textureFormat}");

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

		private static void CheckEqual(byte[] left, byte[] right)
		{
			if (left == null)
			{
				Logger.Verbose("In byte array comparison, left was null");
				return;
			}
			if (right == null)
			{
				Logger.Verbose("In byte array comparison, left was null");
				return;
			}
			if (left.Length != right.Length)
			{
				Logger.Verbose("In byte array comparison, lengths were inequal");
				Logger.Verbose($"Left: {left.Length}");
				Logger.Verbose($"Right: {right.Length}");
				return;
			}
			int length = left.Length;
			int count = 0;
			for (int i = 0; i < length; i++)
			{
				if (left[i] != right[i])
				{
					count++;
				}
			}
			if (count == 0)
			{
				Logger.Verbose("Byte arrays were equal at all indices!");
			}
			else
			{
				Logger.Verbose($"Byte arrays were inequal in {count}/{length} places!");
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
