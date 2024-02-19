using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Classes.ClassID_187;
using AssetRipper.SourceGenerated.Classes.ClassID_188;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_89;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.TextureDecoder.Astc;
using AssetRipper.TextureDecoder.Atc;
using AssetRipper.TextureDecoder.Bc;
using AssetRipper.TextureDecoder.Dxt;
using AssetRipper.TextureDecoder.Etc;
using AssetRipper.TextureDecoder.Pvrtc;
using AssetRipper.TextureDecoder.Rgb;
using AssetRipper.TextureDecoder.Rgb.Formats;
using AssetRipper.TextureDecoder.Yuy2;
using DirectBitmap = AssetRipper.Export.UnityProjects.Utils.DirectBitmap<AssetRipper.TextureDecoder.Rgb.Formats.ColorBGRA32, byte>;

namespace AssetRipper.Export.UnityProjects.Textures
{
	public static class TextureConverter
	{
		public static bool TryConvertToBitmap(ITexture3D texture, out DirectBitmap bitmap)
		{
			byte[] buffer = texture.GetImageData();
			if (buffer.Length == 0)
			{
				bitmap = default;
				return false;
			}

			if (!TryConvertToBitmap(
				texture.GetTextureFormat(),
				texture.Width,
				texture.Height,
				texture.ImageCount,
				texture.GetCompleteImageSize(),
				texture.Collection.Version,
				buffer,
				out bitmap))
			{
				return false;
			}

			bitmap.FlipY();

			// despite the name, this packing works for different formats
			if (texture.LightmapFormatE == TextureUsageMode.NormalmapDXT5nm)
			{
				UnpackNormal(bitmap.Bits);
			}

			return true;
		}

		public static bool TryConvertToBitmap(ITexture2DArray texture, out DirectBitmap bitmap)
		{
			byte[] buffer = texture.GetImageData();
			if (buffer.Length == 0)
			{
				bitmap = default;
				return false;
			}

			if (!TryConvertToBitmap(
				texture.FormatE,
				texture.Width,
				texture.Height,
				texture.Depth,
				(int)texture.DataSize,
				texture.Collection.Version,
				buffer,
				out bitmap))
			{
				return false;
			}

			bitmap.FlipY();

			return true;
		}

		public static bool TryConvertToBitmap(ICubemapArray texture, out DirectBitmap bitmap)
		{
			byte[] buffer = texture.GetImageData();
			if (buffer.Length == 0)
			{
				bitmap = default;
				return false;
			}

			if (!TryConvertToBitmap(
				texture.FormatE,
				texture.Width,
				texture.Width,//Not sure if this is correct
				texture.CubemapCount * 6,//Not sure if this is correct
				(int)texture.DataSize,
				texture.Collection.Version,
				buffer,
				out bitmap))
			{
				return false;
			}

			bitmap.FlipY();

			return true;
		}

		public static bool TryConvertToBitmap(ITexture2D texture, out DirectBitmap bitmap)
		{
			byte[] buffer = texture.GetImageData();
			if (buffer.Length == 0)
			{
				bitmap = default;
				return false;
			}

			if (!TryConvertToBitmap(
				texture.Format_C28E,
				texture.Width_C28,
				texture.Height_C28,
				texture.ImageCount_C28,
				texture.GetCompleteImageSize(),
				texture.Collection.Version,
				buffer,
				out bitmap))
			{
				return false;
			}

			// cubemaps dont need flipping, for some reason
			if (texture is not ICubemap)
			{
				bitmap.FlipY();
			}

			// despite the name, this packing works for different formats
			if (texture.LightmapFormat_C28E == TextureUsageMode.NormalmapDXT5nm)
			{
				UnpackNormal(bitmap.Bits);
			}

			return true;
		}

		private static bool TryConvertToBitmap(
			TextureFormat textureFormat,
			int width,
			int height,
			int depth,
			int imageSize,
			UnityVersion version,
			byte[] data,
			out DirectBitmap bitmap)
		{
			if (width <= 0 || height <= 0 || depth <= 0)
			{
				bitmap = default;
				return false;
			}

			bitmap = new DirectBitmap(width, height, depth);
			int outputSize = width * height * DirectBitmap.PixelSize;
			for (int i = 0; i < depth; i++)
			{
				ReadOnlySpan<byte> inputSpan = new ReadOnlySpan<byte>(data, i * imageSize, imageSize);
				ReadOnlySpan<byte> uncompressedSpan;
				if (textureFormat.IsCrunched())
				{
					if (CrunchHandler.DecompressCrunch(textureFormat, version, inputSpan, out byte[]? decompressedData))
					{
						uncompressedSpan = decompressedData;
					}
					else
					{
						bitmap = default;
						return false;
					}
				}
				else
				{
					uncompressedSpan = inputSpan;
				}
				Span<byte> outputSpan = bitmap.Bits.Slice(i * outputSize, outputSize);

				if (!TryDecodeTexture(textureFormat, width, height, uncompressedSpan, outputSpan))
				{
					bitmap = default;
					return false;
				}
			}
			return true;
		}

		private static bool TryDecodeTexture(TextureFormat textureFormat, int width, int height, ReadOnlySpan<byte> inputSpan, Span<byte> outputSpan)
		{
			switch (textureFormat)
			{
				//ASTC
				case TextureFormat.ASTC_RGB_4x4:
				case TextureFormat.ASTC_RGBA_4x4:
					AstcDecoder.DecodeASTC(inputSpan, width, height, 4, 4, outputSpan);
					return true;

				case TextureFormat.ASTC_RGB_5x5:
				case TextureFormat.ASTC_RGBA_5x5:
					AstcDecoder.DecodeASTC(inputSpan, width, height, 5, 5, outputSpan);
					return true;

				case TextureFormat.ASTC_RGB_6x6:
				case TextureFormat.ASTC_RGBA_6x6:
					AstcDecoder.DecodeASTC(inputSpan, width, height, 6, 6, outputSpan);
					return true;

				case TextureFormat.ASTC_RGB_8x8:
				case TextureFormat.ASTC_RGBA_8x8:
					AstcDecoder.DecodeASTC(inputSpan, width, height, 8, 8, outputSpan);
					return true;

				case TextureFormat.ASTC_RGB_10x10:
				case TextureFormat.ASTC_RGBA_10x10:
					AstcDecoder.DecodeASTC(inputSpan, width, height, 10, 10, outputSpan);
					return true;

				case TextureFormat.ASTC_RGB_12x12:
				case TextureFormat.ASTC_RGBA_12x12:
					AstcDecoder.DecodeASTC(inputSpan, width, height, 12, 12, outputSpan);
					return true;

				//ATC
				case TextureFormat.ATC_RGB4:
					AtcDecoder.DecompressAtcRgb4(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.ATC_RGBA8:
					AtcDecoder.DecompressAtcRgba8(inputSpan, width, height, outputSpan);
					return true;

				//BC
				case TextureFormat.BC4:
				case TextureFormat.BC5:
				case TextureFormat.BC6H:
				case TextureFormat.BC7:
					return DecodeBC(inputSpan, textureFormat, width, height, outputSpan);

				//DXT
				case TextureFormat.DXT1:
				case TextureFormat.DXT1Crunched:
					DxtDecoder.DecompressDXT1(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.DXT3:
					DxtDecoder.DecompressDXT3(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.DXT5:
				case TextureFormat.DXT5Crunched:
					DxtDecoder.DecompressDXT5(inputSpan, width, height, outputSpan);
					return true;

				//ETC
				case TextureFormat.ETC_RGB4:
				case TextureFormat.ETC_RGB4_3DS:
				case TextureFormat.ETC_RGB4Crunched:
					EtcDecoder.DecompressETC(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.EAC_R:
					EtcDecoder.DecompressEACRUnsigned(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.EAC_R_SIGNED:
					EtcDecoder.DecompressEACRSigned(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.EAC_RG:
					EtcDecoder.DecompressEACRGUnsigned(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.EAC_RG_SIGNED:
					EtcDecoder.DecompressEACRGSigned(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.ETC2_RGB:
					EtcDecoder.DecompressETC2(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.ETC2_RGBA1:
					EtcDecoder.DecompressETC2A1(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.ETC2_RGBA8:
				case TextureFormat.ETC_RGBA8_3DS:
				case TextureFormat.ETC2_RGBA8Crunched:
					EtcDecoder.DecompressETC2A8(inputSpan, width, height, outputSpan);
					return true;

				//PVRTC
				case TextureFormat.PVRTC_RGB2:
				case TextureFormat.PVRTC_RGBA2:
					PvrtcDecoder.DecompressPVRTC(inputSpan, width, height, true, outputSpan);
					return true;

				case TextureFormat.PVRTC_RGB4:
				case TextureFormat.PVRTC_RGBA4:
					PvrtcDecoder.DecompressPVRTC(inputSpan, width, height, false, outputSpan);
					return true;

				//YUY2
				case TextureFormat.YUY2:
					Yuy2Decoder.DecompressYUY2(inputSpan, width, height, outputSpan);
					return true;

				//RGB
				case TextureFormat.Alpha8:
					RgbConverter.Convert<ColorA<byte>, byte, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.ARGB4444:
					RgbConverter.Convert<ColorARGB16, byte, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.RGBA4444:
					RgbConverter.Convert<ColorRGBA16, byte, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.RGB565:
					RgbConverter.Convert<ColorRGB16, byte, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.R8:
					RgbConverter.Convert<ColorR<byte>, byte, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.RG16:
					RgbConverter.Convert<ColorRG<byte>, byte, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.RGB24:
					RgbConverter.Convert<ColorRGB<byte>, byte, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.RGBA32:
					RgbConverter.Convert<ColorRGBA<byte>, byte, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.ARGB32:
					RgbConverter.Convert<ColorARGB32, byte, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.BGRA32_14:
				case TextureFormat.BGRA32_37:
					//This needs sliced because the inputSpan can have mips.
					inputSpan[..outputSpan.Length].CopyTo(outputSpan);
					return true;

				case TextureFormat.R16:
					RgbConverter.Convert<ColorR<ushort>, ushort, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.RG32:
					RgbConverter.Convert<ColorRG<ushort>, ushort, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.RGB48:
					RgbConverter.Convert<ColorRGB<ushort>, ushort, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.RGBA64:
					RgbConverter.Convert<ColorRGBA<ushort>, ushort, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.RHalf:
					RgbConverter.Convert<ColorR<Half>, Half, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.RGHalf:
					RgbConverter.Convert<ColorRG<Half>, Half, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.RGBAHalf:
					RgbConverter.Convert<ColorRGBA<Half>, Half, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.RFloat:
					RgbConverter.Convert<ColorR<float>, float, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.RGFloat:
					RgbConverter.Convert<ColorRG<float>, float, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.RGBAFloat:
					RgbConverter.Convert<ColorRGBA<float>, float, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				case TextureFormat.RGB9e5Float:
					RgbConverter.Convert<ColorRGB9e5, double, ColorBGRA32, byte>(inputSpan, width, height, outputSpan);
					return true;

				default:
					Logger.Log(LogType.Error, LogCategory.Export, $"Unsupported texture format '{textureFormat}'");
					return false;
			}
		}

		private static bool DecodeBC(ReadOnlySpan<byte> inputData, TextureFormat textureFormat, int width, int height, Span<byte> outputData)
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

		private static bool NativeDecodeBC(ReadOnlySpan<byte> inputData, TextureFormat textureFormat, int width, int height, Span<byte> outputData)
		{
			Logger.Info(LogCategory.Export, $"Performing alternate decoding for {textureFormat}");

			return textureFormat switch
			{
				TextureFormat.BC4 => Texture2DDecoder.TextureDecoder.DecodeBC4(inputData.ToArray(), width, height, outputData),
				TextureFormat.BC5 => Texture2DDecoder.TextureDecoder.DecodeBC5(inputData.ToArray(), width, height, outputData),
				TextureFormat.BC6H => Texture2DDecoder.TextureDecoder.DecodeBC6(inputData.ToArray(), width, height, outputData),
				TextureFormat.BC7 => Texture2DDecoder.TextureDecoder.DecodeBC7(inputData.ToArray(), width, height, outputData),
				_ => false,
			};
		}

		private static bool ManagedDecodeBC(ReadOnlySpan<byte> inputData, TextureFormat textureFormat, int width, int height, Span<byte> outputData)
		{
			switch (textureFormat)
			{
				case TextureFormat.BC4:
					Bc4.Decompress(inputData, width, height, outputData);
					return true;
				case TextureFormat.BC5:
					Bc5.Decompress(inputData, width, height, outputData);
					return true;
				case TextureFormat.BC6H:
					Bc6h.Decompress(inputData, width, height, false, outputData);
					return true;
				case TextureFormat.BC7:
					Bc7.Decompress(inputData, width, height, outputData);
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
