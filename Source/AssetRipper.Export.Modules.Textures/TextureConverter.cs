using AssetRipper.Export.UnityProjects.Textures;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Classes.ClassID_187;
using AssetRipper.SourceGenerated.Classes.ClassID_188;
using AssetRipper.SourceGenerated.Classes.ClassID_189;
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
using System.Runtime.CompilerServices;

namespace AssetRipper.Export.Modules.Textures;

public static class TextureConverter
{
	public static bool TryConvertToBitmap(IImageTexture texture, out DirectBitmap bitmap)
	{
		return texture switch
		{
			ICubemapArray cubemapArray => TryConvertToBitmap(cubemapArray, out bitmap),
			ITexture2DArray texture2DArray => TryConvertToBitmap(texture2DArray, out bitmap),
			ITexture3D texture3D => TryConvertToBitmap(texture3D, out bitmap),
			ITexture2D texture2D => TryConvertToBitmap(texture2D, out bitmap),
			_ => ReturnFalse(out bitmap),
		};

		static bool ReturnFalse(out DirectBitmap bitmap)
		{
			bitmap = DirectBitmap.Empty;
			return false;
		}
	}

	public static bool TryConvertToBitmap(ITexture3D texture, out DirectBitmap bitmap)
	{
		byte[] buffer = texture.GetImageData();
		if (buffer.Length == 0)
		{
			bitmap = DirectBitmap.Empty;
			return false;
		}

		if (!TryGetTextureFormat((GraphicsFormat)texture.Format, out TextureFormat format))
		{
			bitmap = DirectBitmap.Empty;
			return false;
		}

		if (!TryConvertToBitmap(
			format,
			texture.Width,
			texture.Height,
			texture.Depth,
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
			bitmap = DirectBitmap.Empty;
			return false;
		}

		if (!TryGetTextureFormat((GraphicsFormat)texture.Format, out TextureFormat format))
		{
			bitmap = DirectBitmap.Empty;
			return false;
		}

		if (!TryConvertToBitmap(
			format,
			texture.Width,
			texture.Height,
			texture.Depth,
			texture.GetCompleteImageSize(),
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
			bitmap = DirectBitmap.Empty;
			return false;
		}

		if (!TryGetTextureFormat((GraphicsFormat)texture.Format, out TextureFormat format))
		{
			bitmap = DirectBitmap.Empty;
			return false;
		}

		if (!TryConvertToBitmap(
			format,
			texture.Width,
			texture.GetHeight(),
			texture.GetDepth(),
			texture.GetCompleteImageSize(),
			texture.Collection.Version,
			buffer,
			out bitmap))
		{
			return false;
		}

		bitmap.FlipY();// Maybe not needed?

		return true;
	}

	public static bool TryConvertToBitmap(ITexture2D texture, out DirectBitmap bitmap)
	{
		byte[] buffer = texture.GetImageData();
		if (buffer.Length == 0)
		{
			bitmap = DirectBitmap.Empty;
			return false;
		}

		if (!TryConvertToBitmap(
			texture.Format_C28E,
			texture.Width_C28,
			texture.Height_C28,
			texture.ImageCount_C28,
			texture.ActualImageSize,
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
		return textureFormat switch
		{
			TextureFormat.Alpha8 => TryConvertToBitmap<ColorA<byte>, byte>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.ARGB4444 => TryConvertToBitmap<ColorARGB16, byte>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RGB24 => TryConvertToBitmap<ColorRGB<byte>, byte>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RGBA32 => TryConvertToBitmap<ColorRGBA<byte>, byte>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.ARGB32 => TryConvertToBitmap<ColorARGB32, byte>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RGB565 => TryConvertToBitmap<ColorRGB16, byte>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.R16 => TryConvertToBitmap<ColorR<ushort>, ushort>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RGBA4444 => TryConvertToBitmap<ColorRGBA16, byte>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.BGRA32_14 or TextureFormat.BGRA32_37 => TryConvertToBitmap<ColorBGRA32, byte>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RHalf => TryConvertToBitmap<ColorR<Half>, Half>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RGHalf => TryConvertToBitmap<ColorRG<Half>, Half>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RGBAHalf => TryConvertToBitmap<ColorRGBA<Half>, Half>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RFloat => TryConvertToBitmap<ColorR<float>, float>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RGFloat => TryConvertToBitmap<ColorRG<float>, float>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RGBAFloat => TryConvertToBitmap<ColorRGBA<float>, float>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RGB9e5Float => TryConvertToBitmap<ColorRGB9e5, double>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RG16 => TryConvertToBitmap<ColorRG<byte>, byte>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.R8 => TryConvertToBitmap<ColorR<byte>, byte>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RG32 => TryConvertToBitmap<ColorRG<ushort>, ushort>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RGB48 => TryConvertToBitmap<ColorRGB<ushort>, ushort>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RGBA64 => TryConvertToBitmap<ColorRGBA<ushort>, ushort>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.R8_SIGNED => TryConvertToBitmap<ColorR<sbyte>, sbyte>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RG16_SIGNED => TryConvertToBitmap<ColorRG<sbyte>, sbyte>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RGB24_SIGNED => TryConvertToBitmap<ColorRGB<sbyte>, sbyte>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RGBA32_SIGNED => TryConvertToBitmap<ColorRGBA<sbyte>, sbyte>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.R16_SIGNED => TryConvertToBitmap<ColorR<short>, short>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RG32_SIGNED => TryConvertToBitmap<ColorRG<short>, short>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RGB48_SIGNED => TryConvertToBitmap<ColorRGB<short>, short>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			TextureFormat.RGBA64_SIGNED => TryConvertToBitmap<ColorRGBA<short>, short>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
			_ => TryConvertToBitmap<ColorBGRA32, byte>(textureFormat, width, height, depth, imageSize, version, data, out bitmap),
		};
	}

	private static bool TryConvertToBitmap<TColor, TChannelValue>(
		TextureFormat textureFormat,
		int width,
		int height,
		int depth,
		int imageSize,
		UnityVersion version,
		byte[] data,
		out DirectBitmap bitmap)
		where TColor : unmanaged, IColor<TChannelValue>
		where TChannelValue : unmanaged
	{
		if (width <= 0 || height <= 0 || depth <= 0)
		{
			Logger.Log(LogType.Error, LogCategory.Export, $"Invalid texture dimensions. Width: {width}, Height: {height}, Depth: {depth}.");
			bitmap = DirectBitmap.Empty;
			return false;
		}

		if (1L * width * height * depth * Unsafe.SizeOf<TColor>() > int.MaxValue)
		{
			Logger.Log(LogType.Error, LogCategory.Export, $"Texture size is too large. Width: {width}, Height: {height}, Depth: {depth}.");
			bitmap = DirectBitmap.Empty;
			return false;
		}

		if (data.Length < (long)imageSize * depth)
		{
			Logger.Log(LogType.Error, LogCategory.Export, $"Image data length {data.Length} is less than expected {(long)imageSize * depth}. Width: {width}, Height: {height}, Depth: {depth}, Image Size: {imageSize}, Format {textureFormat}.");
			bitmap = DirectBitmap.Empty;
			return false;
		}

		ReadOnlySpan<byte> uncompressedSpan;
		int bytesPerLayer;
		if (textureFormat.IsCrunched())
		{
			if (CrunchHandler.DecompressCrunch(textureFormat, version, data, out byte[]? decompressedData))
			{
				uncompressedSpan = decompressedData;
				bytesPerLayer = decompressedData.Length / depth;
			}
			else
			{
				bitmap = DirectBitmap.Empty;
				return false;
			}
		}
		else
		{
			uncompressedSpan = data;
			bytesPerLayer = imageSize;
		}

		bitmap = new DirectBitmap<TColor, TChannelValue>(width, height, depth);
		int outputSize = width * height * bitmap.PixelSize;
		for (int i = 0; i < depth; i++)
		{
			ReadOnlySpan<byte> inputSpan = uncompressedSpan.Slice(i * bytesPerLayer, bytesPerLayer);
			Span<byte> outputSpan = bitmap.Bits.Slice(i * outputSize, outputSize);

			if (typeof(TColor) == typeof(ColorBGRA32))
			{
				if (!TryDecodeTexture(textureFormat, width, height, inputSpan, outputSpan))
				{
					bitmap = DirectBitmap.Empty;
					return false;
				}
			}
			else
			{
				if (!TryDecodeTexture<TColor, TChannelValue>(textureFormat, width, height, inputSpan, outputSpan))
				{
					bitmap = DirectBitmap.Empty;
					return false;
				}
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
				Bc4.Decompress(inputSpan, width, height, outputSpan);
				return true;
			case TextureFormat.BC5:
				Bc5.Decompress(inputSpan, width, height, outputSpan);
				return true;
			case TextureFormat.BC6H:
				Bc6h.Decompress(inputSpan, width, height, false, outputSpan);
				return true;
			case TextureFormat.BC7:
				Bc7.Decompress(inputSpan, width, height, outputSpan);
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

			case TextureFormat.BGRA32_14:
			case TextureFormat.BGRA32_37:
				//This needs sliced because the inputSpan can have mips.
				inputSpan[..outputSpan.Length].CopyTo(outputSpan);
				return true;

			default:
				return TryDecodeTexture<ColorBGRA32, byte>(textureFormat, width, height, inputSpan, outputSpan);
		}
	}

	private static bool TryDecodeTexture<TColor, TChannelValue>(TextureFormat textureFormat, int width, int height, ReadOnlySpan<byte> inputSpan, Span<byte> outputSpan)
		where TColor : unmanaged, IColor<TChannelValue>
		where TChannelValue : unmanaged
	{
		switch (textureFormat)
		{
			//DXT
			case TextureFormat.DXT1:
			case TextureFormat.DXT1Crunched:
				DxtDecoder.DecompressDXT1<TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.DXT3:
				DxtDecoder.DecompressDXT3<TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.DXT5:
			case TextureFormat.DXT5Crunched:
				DxtDecoder.DecompressDXT5<TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			//YUY2
			case TextureFormat.YUY2:
				Yuy2Decoder.DecompressYUY2<TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			//RGB
			case TextureFormat.Alpha8:
				RgbConverter.Convert<ColorA<byte>, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.ARGB4444:
				RgbConverter.Convert<ColorARGB16, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RGBA4444:
				RgbConverter.Convert<ColorRGBA16, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RGB565:
				RgbConverter.Convert<ColorRGB16, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.R8:
				RgbConverter.Convert<ColorR<byte>, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RG16:
				RgbConverter.Convert<ColorRG<byte>, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RGB24:
				RgbConverter.Convert<ColorRGB<byte>, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RGBA32:
				RgbConverter.Convert<ColorRGBA<byte>, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.ARGB32:
				RgbConverter.Convert<ColorARGB32, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.BGRA32_14:
			case TextureFormat.BGRA32_37:
				RgbConverter.Convert<ColorBGRA32, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.R16:
				RgbConverter.Convert<ColorR<ushort>, ushort, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RG32:
				RgbConverter.Convert<ColorRG<ushort>, ushort, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RGB48:
				RgbConverter.Convert<ColorRGB<ushort>, ushort, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RGBA64:
				RgbConverter.Convert<ColorRGBA<ushort>, ushort, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RHalf:
				RgbConverter.Convert<ColorR<Half>, Half, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RGHalf:
				RgbConverter.Convert<ColorRG<Half>, Half, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RGBAHalf:
				RgbConverter.Convert<ColorRGBA<Half>, Half, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RFloat:
				RgbConverter.Convert<ColorR<float>, float, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RGFloat:
				RgbConverter.Convert<ColorRG<float>, float, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RGBAFloat:
				RgbConverter.Convert<ColorRGBA<float>, float, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RGB9e5Float:
				RgbConverter.Convert<ColorRGB9e5, double, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.R8_SIGNED:
				RgbConverter.Convert<ColorR<sbyte>, sbyte, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RG16_SIGNED:
				RgbConverter.Convert<ColorRG<sbyte>, sbyte, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RGB24_SIGNED:
				RgbConverter.Convert<ColorRGB<sbyte>, sbyte, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RGBA32_SIGNED:
				RgbConverter.Convert<ColorRGBA<sbyte>, sbyte, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.R16_SIGNED:
				RgbConverter.Convert<ColorR<short>, short, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RG32_SIGNED:
				RgbConverter.Convert<ColorRG<short>, short, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RGB48_SIGNED:
				RgbConverter.Convert<ColorRGB<short>, short, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			case TextureFormat.RGBA64_SIGNED:
				RgbConverter.Convert<ColorRGBA<short>, short, TColor, TChannelValue>(inputSpan, width, height, outputSpan);
				return true;

			default:
				Logger.Log(LogType.Error, LogCategory.Export, $"Unsupported texture format '{textureFormat}'");
				return false;
		}
	}

	private static bool TryGetTextureFormat(GraphicsFormat graphicsFormat, out TextureFormat format)
	{
		try
		{
			format = graphicsFormat.ToTextureFormat();
			return true;
		}
		catch (NotSupportedException)
		{
			format = default;
			return false;
		}
		catch (ArgumentOutOfRangeException)
		{
			Logger.Log(LogType.Error, LogCategory.Export, $"Unknown GraphicsFormat '{(int)graphicsFormat}'");
			format = default;
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
			double vr = r * 2.0 - 255.0;
			double vg = g * 2.0 - 255.0;
			double hypotenuseSqr = Math.Min(vr * vr + vg * vg, MagnitudeSqr);
			double b = (Math.Sqrt(MagnitudeSqr - hypotenuseSqr) + 255.0) / 2.0;
			pixelSpan[0] = (byte)b;
		}
	}
}
