using AssetRipper.Core.Logging;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Library.Exporters.Textures.Extensions;
using AssetRipper.Library.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
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
		public static DirectBitmap? ConvertToBitmap(ITexture2D texture)
		{
			byte[] buffer = texture.GetImageData();
			if (buffer.Length == 0)
			{
				return null;
			}

			int pvrtcBitCount = texture.PVRTCBitCount(true);
			int astcBlockSize = texture.ASTCBlockSize(true);

			DirectBitmap? bitmap = ConvertToBitmap(texture.Format_C28E, texture.Width_C28, texture.Height_C28, texture.Collection.Version, buffer, pvrtcBitCount, astcBlockSize);

			if (bitmap == null)
			{
				return null;
			}

			// despite the name, this packing works for different formats
			if (texture.LightmapFormat_C28 == (int)TextureUsageMode.NormalmapDXT5nm)
			{
				UnpackNormal(bitmap.Bits);
			}

			return bitmap;
		}

		private static DirectBitmap? ConvertToBitmap(TextureFormat textureFormat, int width, int height, UnityVersion version, byte[] data, int pvrtcBitCount, int astcBlockSize)
		{
			if (width == 0 || height == 0)
			{
				return new DirectBitmap(1, 1);
			}

			switch (textureFormat)
			{
				case TextureFormat.DXT1:
				case TextureFormat.DXT3:
				case TextureFormat.DXT5:
					return DXTTextureToBitmap(textureFormat, width, height, data);

				case TextureFormat.Alpha8:
				case TextureFormat.ARGB4444:
				case TextureFormat.RGB24:
				case TextureFormat.RGBA32:
				case TextureFormat.ARGB32:
				case TextureFormat.RGB565:
				case TextureFormat.R16:
				case TextureFormat.RGBA4444:
				case TextureFormat.BGRA32_14:
				case TextureFormat.RHalf:
				case TextureFormat.RGHalf:
				case TextureFormat.RGBAHalf:
				case TextureFormat.RFloat:
				case TextureFormat.RGFloat:
				case TextureFormat.RGBAFloat:
				case TextureFormat.RGB9e5Float:
				case TextureFormat.RG16:
				case TextureFormat.R8:
					return RGBTextureToBitmap(textureFormat, width, height, data);

				case TextureFormat.YUY2:
					return YUY2TextureToBitmap(textureFormat, width, height, data);

				case TextureFormat.PVRTC_RGB2:
				case TextureFormat.PVRTC_RGBA2:
				case TextureFormat.PVRTC_RGB4:
				case TextureFormat.PVRTC_RGBA4:
					return PVRTCTextureToBitmap(pvrtcBitCount, textureFormat, width, height, data);

				case TextureFormat.ETC_RGB4:
				case TextureFormat.EAC_R:
				case TextureFormat.EAC_R_SIGNED:
				case TextureFormat.EAC_RG:
				case TextureFormat.EAC_RG_SIGNED:
				case TextureFormat.ETC2_RGB:
				case TextureFormat.ETC2_RGBA1:
				case TextureFormat.ETC2_RGBA8:
				case TextureFormat.ETC_RGB4_3DS:
				case TextureFormat.ETC_RGBA8_3DS:
					return ETCTextureToBitmap(textureFormat, width, height, data);

				case TextureFormat.ATC_RGB4_35:
				case TextureFormat.ATC_RGBA8_36:
					return ATCTextureToBitmap(textureFormat, width, height, data);

				case TextureFormat.ASTC_RGB_4x4:
				case TextureFormat.ASTC_RGB_5x5:
				case TextureFormat.ASTC_RGB_6x6:
				case TextureFormat.ASTC_RGB_8x8:
				case TextureFormat.ASTC_RGB_10x10:
				case TextureFormat.ASTC_RGB_12x12:
				case TextureFormat.ASTC_RGBA_4x4:
				case TextureFormat.ASTC_RGBA_5x5:
				case TextureFormat.ASTC_RGBA_6x6:
				case TextureFormat.ASTC_RGBA_8x8:
				case TextureFormat.ASTC_RGBA_10x10:
				case TextureFormat.ASTC_RGBA_12x12:
					return ASTCTextureToBitmap(astcBlockSize, width, height, data);

				case TextureFormat.BC4:
				case TextureFormat.BC5:
				case TextureFormat.BC6H:
				case TextureFormat.BC7:
					return BcTextureToBitmap(textureFormat, width, height, data);

				case TextureFormat.DXT1Crunched:
				case TextureFormat.DXT5Crunched:
					return DXTCrunchedTextureToBitmap(textureFormat, width, height, version, data);

				case TextureFormat.ETC_RGB4Crunched:
				case TextureFormat.ETC2_RGBA8Crunched:
					return ETCCrunchedTextureToBitmap(textureFormat, width, height, version, data);

				default:
					Logger.Log(LogType.Error, LogCategory.Export, $"Unsupported texture format '{textureFormat}'");
					return null;
			}
		}

		private static DirectBitmap DXTTextureToBitmap(TextureFormat textureFormat, int width, int height, byte[] data)
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

		private static DirectBitmap DXTCrunchedTextureToBitmap(TextureFormat textureFormat, int width, int height, UnityVersion unityVersion, byte[] data)
		{
			byte[] decompressed = CrunchHandler.DecompressCrunch(textureFormat, width, height, unityVersion, data);
			return DXTTextureToBitmap(textureFormat, width, height, decompressed);
		}

		private static DirectBitmap RGBTextureToBitmap(TextureFormat textureFormat, int width, int height, byte[] data)
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

		private static DirectBitmap ETCTextureToBitmap(TextureFormat textureFormat, int width, int height, byte[] data)
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

		private static DirectBitmap ETCCrunchedTextureToBitmap(TextureFormat textureFormat, int width, int height, UnityVersion unityVersion, byte[] data)
		{
			byte[] decompressed = CrunchHandler.DecompressCrunch(textureFormat, width, height, unityVersion, data);
			return ETCTextureToBitmap(textureFormat, width, height, decompressed);
		}

		private static DirectBitmap ATCTextureToBitmap(TextureFormat textureFormat, int width, int height, byte[] data)
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

		private static DirectBitmap YUY2TextureToBitmap(TextureFormat textureFormat, int width, int height, byte[] data)
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

		private static DirectBitmap PVRTCTextureToBitmap(int bitCount, TextureFormat textureFormat, int width, int height, byte[] data)
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

		private static DirectBitmap ASTCTextureToBitmap(int blockSize, int width, int height, byte[] data)
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

		private static DirectBitmap BcTextureToBitmap(TextureFormat textureFormat, int width, int height, byte[] data)
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

		private static bool DecodeBC(byte[] inputData, TextureFormat textureFormat, int width, int height, byte[] outputData)
		{
			if (width % 4 != 0 || height % 4 != 0) //Managed code doesn't currently handle partial block sizes well.
			{
				return NativeDecodeBC(inputData, textureFormat, width, height, outputData);
			}
			else
			{
				return ManagedDecodeBC(inputData, textureFormat, width, height, outputData);
			}
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

		private static void UnpackNormal(Span<byte> data)
		{
			for (int i = 0; i < data.Length; i += 4)
			{
				Span<byte> pixelSpan = data.Slice(i, 4);
				byte r = pixelSpan[3];
				byte g = pixelSpan[1];
				byte a = pixelSpan[2];
				pixelSpan[2] = r;
				pixelSpan[3] = a;

				const double MagnitudeSqr = 255 * 255;
				double vr = (r * 2.0) - 255.0;
				double vg = (g * 2.0) - 255.0;
				double hypotenuseSqr = Math.Min((vr * vr) + (vg * vg), MagnitudeSqr);
				double b = (Math.Sqrt(MagnitudeSqr - hypotenuseSqr) + 255.0) / 2.0;
				pixelSpan[0] = (byte)b;
			}
		}
	}
}
