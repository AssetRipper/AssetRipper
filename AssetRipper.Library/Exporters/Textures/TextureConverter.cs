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
using AssetRipper.TextureDecoder.Rgb.Formats;
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

			DirectBitmap? bitmap = ConvertToBitmap(texture.Format_C28E, texture.Width_C28, texture.Height_C28, texture.Collection.Version, buffer);

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

		private static DirectBitmap? ConvertToBitmap(TextureFormat textureFormat, int width, int height, UnityVersion version, byte[] data)
		{
			if (width == 0 || height == 0)
			{
				return new DirectBitmap(1, 1);
			}

			if (textureFormat
				is TextureFormat.DXT1Crunched
				or TextureFormat.DXT5Crunched
				or TextureFormat.ETC_RGB4Crunched
				or TextureFormat.ETC2_RGBA8Crunched)
			{
				data = CrunchHandler.DecompressCrunch(textureFormat, width, height, version, data);
			}

			DirectBitmap bitmap = new DirectBitmap(width, height);
			try
			{
				switch (textureFormat)
				{
					//ASTC
					case TextureFormat.ASTC_RGB_4x4:
					case TextureFormat.ASTC_RGBA_4x4:
						AstcDecoder.DecodeASTC(data, width, height, 4, 4, bitmap.Bits);
						break;

					case TextureFormat.ASTC_RGB_5x5:
					case TextureFormat.ASTC_RGBA_5x5:
						AstcDecoder.DecodeASTC(data, width, height, 5, 5, bitmap.Bits);
						break;

					case TextureFormat.ASTC_RGB_6x6:
					case TextureFormat.ASTC_RGBA_6x6:
						AstcDecoder.DecodeASTC(data, width, height, 6, 6, bitmap.Bits);
						break;

					case TextureFormat.ASTC_RGB_8x8:
					case TextureFormat.ASTC_RGBA_8x8:
						AstcDecoder.DecodeASTC(data, width, height, 8, 8, bitmap.Bits);
						break;

					case TextureFormat.ASTC_RGB_10x10:
					case TextureFormat.ASTC_RGBA_10x10:
						AstcDecoder.DecodeASTC(data, width, height, 10, 10, bitmap.Bits);
						break;

					case TextureFormat.ASTC_RGB_12x12:
					case TextureFormat.ASTC_RGBA_12x12:
						AstcDecoder.DecodeASTC(data, width, height, 12, 12, bitmap.Bits);
						break;

					//ATC
					case TextureFormat.ATC_RGB4_35:
						AtcDecoder.DecompressAtcRgb4(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.ATC_RGBA8_36:
						AtcDecoder.DecompressAtcRgba8(data, width, height, bitmap.Bits);
						break;

					//BC
					case TextureFormat.BC4:
					case TextureFormat.BC5:
					case TextureFormat.BC6H:
					case TextureFormat.BC7:
						DecodeBC(data, textureFormat, width, height, bitmap.Bits);
						break;

					//DXT
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

					//ETC
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

					//PVRTC
					case TextureFormat.PVRTC_RGB2:
					case TextureFormat.PVRTC_RGBA2:
						PvrtcDecoder.DecompressPVRTC(data, width, height, true, bitmap.Bits);
						break;

					case TextureFormat.PVRTC_RGB4:
					case TextureFormat.PVRTC_RGBA4:
						PvrtcDecoder.DecompressPVRTC(data, width, height, false, bitmap.Bits);
						break;

					//YUY2
					case TextureFormat.YUY2:
						Yuy2Decoder.DecompressYUY2(data, width, height, bitmap.Bits);
						break;

					//RGB
					case TextureFormat.Alpha8:
						RgbConverter.Convert<ColorA8, byte, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.ARGB4444:
						RgbConverter.Convert<ColorARGB16, byte, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.RGBA4444:
						RgbConverter.Convert<ColorRGBA16, byte, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.RGB565:
						RgbConverter.Convert<ColorRGB16, byte, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.R8:
						RgbConverter.Convert<ColorR8, byte, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.RG16:
						RgbConverter.Convert<ColorRG16, byte, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.RGB24:
						RgbConverter.Convert<ColorRGB24, byte, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.RGBA32:
						RgbConverter.Convert<ColorRGBA32, byte, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.ARGB32:
						RgbConverter.Convert<ColorARGB32, byte, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.BGRA32_14:
					case TextureFormat.BGRA32_37:
						Buffer.BlockCopy(data, 0, bitmap.Bits, 0, bitmap.Bits.Length);
						break;

					case TextureFormat.R16:
						RgbConverter.Convert<ColorR16, ushort, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.RG32:
						RgbConverter.Convert<ColorRG32, ushort, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.RGB48:
						RgbConverter.Convert<ColorRGB48, ushort, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.RGBA64:
						RgbConverter.Convert<ColorRGBA64, ushort, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.RHalf:
						RgbConverter.Convert<ColorRHalf, Half, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.RGHalf:
						RgbConverter.Convert<ColorRGHalf, Half, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.RGBAHalf:
						RgbConverter.Convert<ColorRGBAHalf, Half, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.RFloat:
						RgbConverter.Convert<ColorRSingle, float, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.RGFloat:
						RgbConverter.Convert<ColorRGSingle, float, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.RGBAFloat:
						RgbConverter.Convert<ColorRGBASingle, float, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					case TextureFormat.RGB9e5Float:
						RgbConverter.Convert<ColorRGB9e5, double, ColorBGRA32, byte>(data, width, height, bitmap.Bits);
						break;

					default:
						Logger.Log(LogType.Error, LogCategory.Export, $"Unsupported texture format '{textureFormat}'");
						bitmap.Dispose();
						return null;
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
