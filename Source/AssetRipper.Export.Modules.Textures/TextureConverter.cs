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
	private readonly record struct Options(TextureFormat TextureFormat, int Width, int Height, int Depth, int ImageSize, UnityVersion Version)
	{
		public bool IsCrunched => TextureFormat.IsCrunched();
	}

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

		Options options = new()
		{
			TextureFormat = format,
			Width = texture.Width,
			Height = texture.Height,
			Depth = texture.Depth,
			ImageSize = texture.GetCompleteImageSize(),
			Version = texture.Collection.Version,
		};

		if (!TryConvertToBitmap(options, buffer, out bitmap))
		{
			return false;
		}

		bitmap.FlipY();

		// despite the name, this packing works for different formats
		if (texture.LightmapFormatE == TextureUsageMode.NormalmapDXT5nm)
		{
			UnpackNormal(bitmap);
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

		Options options = new()
		{
			TextureFormat = format,
			Width = texture.Width,
			Height = texture.Height,
			Depth = texture.Depth,
			ImageSize = texture.GetCompleteImageSize(),
			Version = texture.Collection.Version,
		};

		if (!TryConvertToBitmap(options, buffer, out bitmap))
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

		Options options = new()
		{
			TextureFormat = format,
			Width = texture.Width,
			Height = texture.GetHeight(),
			Depth = texture.GetDepth(),
			ImageSize = texture.GetCompleteImageSize(),
			Version = texture.Collection.Version,
		};

		if (!TryConvertToBitmap(options, buffer, out bitmap))
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

		Options options = new()
		{
			TextureFormat = texture.Format_C28E,
			Width = texture.Width_C28,
			Height = texture.Height_C28,
			Depth = texture.ImageCount_C28,
			ImageSize = texture.ActualImageSize,
			Version = texture.Collection.Version,
		};

		if (!TryConvertToBitmap(options, buffer, out bitmap))
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
			UnpackNormal(bitmap);
		}

		return true;
	}

	private static bool TryConvertToBitmap(Options options, byte[] data, out DirectBitmap bitmap)
	{
		return options.TextureFormat switch
		{
			TextureFormat.Alpha8 => TryConvertToBitmap<ColorA<byte>, byte>(options, data, out bitmap),
			TextureFormat.ARGB4444 => TryConvertToBitmap<ColorARGB16, byte>(options, data, out bitmap),
			TextureFormat.RGB24 => TryConvertToBitmap<ColorRGB<byte>, byte>(options, data, out bitmap),
			TextureFormat.RGBA32 => TryConvertToBitmap<ColorRGBA<byte>, byte>(options, data, out bitmap),
			TextureFormat.ARGB32 => TryConvertToBitmap<ColorARGB<byte>, byte>(options, data, out bitmap),
			TextureFormat.ARGBFloat => TryConvertToBitmap<ColorARGB<float>, float>(options, data, out bitmap),
			TextureFormat.RGB565 => TryConvertToBitmap<ColorRGB16, byte>(options, data, out bitmap),
			TextureFormat.BGR24 => TryConvertToBitmap<ColorBGR<byte>, byte>(options, data, out bitmap),
			TextureFormat.R16 => TryConvertToBitmap<ColorR<ushort>, ushort>(options, data, out bitmap),
			TextureFormat.RGBA4444 => TryConvertToBitmap<ColorRGBA16, byte>(options, data, out bitmap),
			TextureFormat.BGRA32_14 or TextureFormat.BGRA32_37 => TryConvertToBitmap<ColorBGRA<byte>, byte>(options, data, out bitmap),
			TextureFormat.RHalf => TryConvertToBitmap<ColorR<Half>, Half>(options, data, out bitmap),
			TextureFormat.RGHalf => TryConvertToBitmap<ColorRG<Half>, Half>(options, data, out bitmap),
			TextureFormat.RGBAHalf => TryConvertToBitmap<ColorRGBA<Half>, Half>(options, data, out bitmap),
			TextureFormat.RFloat => TryConvertToBitmap<ColorR<float>, float>(options, data, out bitmap),
			TextureFormat.RGFloat => TryConvertToBitmap<ColorRG<float>, float>(options, data, out bitmap),
			TextureFormat.RGBAFloat => TryConvertToBitmap<ColorRGBA<float>, float>(options, data, out bitmap),
			TextureFormat.RGB9e5Float => TryConvertToBitmap<ColorRGB9e5, double>(options, data, out bitmap),
			TextureFormat.RG16 => TryConvertToBitmap<ColorRG<byte>, byte>(options, data, out bitmap),
			TextureFormat.R8 => TryConvertToBitmap<ColorR<byte>, byte>(options, data, out bitmap),
			TextureFormat.RG32 => TryConvertToBitmap<ColorRG<ushort>, ushort>(options, data, out bitmap),
			TextureFormat.RGB48 => TryConvertToBitmap<ColorRGB<ushort>, ushort>(options, data, out bitmap),
			TextureFormat.RGBA64 => TryConvertToBitmap<ColorRGBA<ushort>, ushort>(options, data, out bitmap),
			TextureFormat.R8_SIGNED => TryConvertToBitmap<ColorR<sbyte>, sbyte>(options, data, out bitmap),
			TextureFormat.RG16_SIGNED => TryConvertToBitmap<ColorRG<sbyte>, sbyte>(options, data, out bitmap),
			TextureFormat.RGB24_SIGNED => TryConvertToBitmap<ColorRGB<sbyte>, sbyte>(options, data, out bitmap),
			TextureFormat.RGBA32_SIGNED => TryConvertToBitmap<ColorRGBA<sbyte>, sbyte>(options, data, out bitmap),
			TextureFormat.R16_SIGNED => TryConvertToBitmap<ColorR<short>, short>(options, data, out bitmap),
			TextureFormat.RG32_SIGNED => TryConvertToBitmap<ColorRG<short>, short>(options, data, out bitmap),
			TextureFormat.RGB48_SIGNED => TryConvertToBitmap<ColorRGB<short>, short>(options, data, out bitmap),
			TextureFormat.RGBA64_SIGNED => TryConvertToBitmap<ColorRGBA<short>, short>(options, data, out bitmap),
			_ => TryConvertToBitmap<ColorRGBA<byte>, byte>(options, data, out bitmap),
		};
	}

	private static bool TryConvertToBitmap<TColor, TChannelValue>(Options options, byte[] data, out DirectBitmap bitmap)
		where TColor : unmanaged, IColor<TChannelValue>
		where TChannelValue : unmanaged
	{
		if (options.Width <= 0 || options.Height <= 0 || options.Depth <= 0)
		{
			Logger.Log(LogType.Error, LogCategory.Export, $"Invalid texture dimensions. Width: {options.Width}, Height: {options.Height}, Depth: {options.Depth}.");
			bitmap = DirectBitmap.Empty;
			return false;
		}

		if (1L * options.Width * options.Height * options.Depth * Unsafe.SizeOf<TColor>() > int.MaxValue)
		{
			Logger.Log(LogType.Error, LogCategory.Export, $"Texture size is too large. Width: {options.Width}, Height: {options.Height}, Depth: {options.Depth}.");
			bitmap = DirectBitmap.Empty;
			return false;
		}

		ReadOnlySpan<byte> uncompressedSpan;
		int bytesPerLayer;
		if (options.IsCrunched)
		{
			if (CrunchHandler.DecompressCrunch(options.TextureFormat, options.Version, data, out byte[]? decompressedData))
			{
				uncompressedSpan = decompressedData;
				bytesPerLayer = decompressedData.Length / options.Depth;
			}
			else
			{
				bitmap = DirectBitmap.Empty;
				return false;
			}
		}
		else
		{
			if (data.Length == options.ImageSize)
			{
				// This can happen for Texture3D
				// For mips, all 3 dimensions get halved with each mip level, unlike Texture2DArray.
				// https://github.com/AssetRipper/AssetRipper/issues/1886
				bytesPerLayer = -1;
			}
			else if (data.Length < (long)options.ImageSize * options.Depth)
			{
				Logger.Log(LogType.Error, LogCategory.Export, $"Image data length {data.Length} is less than expected {(long)options.ImageSize * options.Depth}. Width: {options.Width}, Height: {options.Height}, Depth: {options.Depth}, Image Size: {options.ImageSize}, Format {options.TextureFormat}.");
				bitmap = DirectBitmap.Empty;
				return false;
			}
			else
			{
				bytesPerLayer = options.ImageSize;
			}
			uncompressedSpan = data;
		}

		bitmap = new DirectBitmap<TColor, TChannelValue>(options.Width, options.Height, options.Depth);
		int outputSize = options.Width * options.Height * bitmap.PixelSize;
		int inputOffset = 0;
		for (int i = 0; i < options.Depth; i++)
		{
			ReadOnlySpan<byte> inputSpan = uncompressedSpan.Slice(inputOffset, int.Max(uncompressedSpan.Length - inputOffset, bytesPerLayer));
			Span<byte> outputSpan = bitmap.Bits.Slice(i * outputSize, outputSize);

			int bytesRead = DecodeTexture<TColor, TChannelValue>(options, inputSpan, outputSpan);
			if (bytesRead < 0)
			{
				bitmap = DirectBitmap.Empty;
				return false;
			}

			inputOffset += bytesPerLayer > 0 ? bytesPerLayer : bytesRead;
		}
		return true;
	}

	private static int DecodeTexture<TColor, TChannelValue>(Options options, ReadOnlySpan<byte> inputSpan, Span<byte> outputSpan)
		where TColor : unmanaged, IColor<TChannelValue>
		where TChannelValue : unmanaged
	{
		int width = options.Width;
		int height = options.Height;
		return options.TextureFormat switch
		{
			//ASTC
			TextureFormat.ASTC_RGB_4x4 or TextureFormat.ASTC_RGBA_4x4 => AstcDecoder.DecodeASTC<TColor, TChannelValue>(inputSpan, width, height, 4, 4, outputSpan),
			TextureFormat.ASTC_RGB_5x5 or TextureFormat.ASTC_RGBA_5x5 => AstcDecoder.DecodeASTC<TColor, TChannelValue>(inputSpan, width, height, 5, 5, outputSpan),
			TextureFormat.ASTC_RGB_6x6 or TextureFormat.ASTC_RGBA_6x6 => AstcDecoder.DecodeASTC<TColor, TChannelValue>(inputSpan, width, height, 6, 6, outputSpan),
			TextureFormat.ASTC_RGB_8x8 or TextureFormat.ASTC_RGBA_8x8 => AstcDecoder.DecodeASTC<TColor, TChannelValue>(inputSpan, width, height, 8, 8, outputSpan),
			TextureFormat.ASTC_RGB_10x10 or TextureFormat.ASTC_RGBA_10x10 => AstcDecoder.DecodeASTC<TColor, TChannelValue>(inputSpan, width, height, 10, 10, outputSpan),
			TextureFormat.ASTC_RGB_12x12 or TextureFormat.ASTC_RGBA_12x12 => AstcDecoder.DecodeASTC<TColor, TChannelValue>(inputSpan, width, height, 12, 12, outputSpan),

			//ATC
			TextureFormat.ATC_RGB4 => AtcDecoder.DecompressAtcRgb4<TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.ATC_RGBA8 => AtcDecoder.DecompressAtcRgba8<TColor, TChannelValue>(inputSpan, width, height, outputSpan),

			//BC
			TextureFormat.BC4 => Bc4.Decompress<TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.BC5 => Bc5.Decompress<TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.BC6H => Bc6h.Decompress<TColor, TChannelValue>(inputSpan, width, height, false, outputSpan),
			TextureFormat.BC7 => Bc7.Decompress<TColor, TChannelValue>(inputSpan, width, height, outputSpan),

			//DXT
			TextureFormat.DXT1 or TextureFormat.DXT1Crunched => DxtDecoder.DecompressDXT1<TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.DXT3 => DxtDecoder.DecompressDXT3<TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.DXT5 or TextureFormat.DXT5Crunched => DxtDecoder.DecompressDXT5<TColor, TChannelValue>(inputSpan, width, height, outputSpan),

			//ETC
			TextureFormat.ETC_RGB4 or TextureFormat.ETC_RGB4_3DS or TextureFormat.ETC_RGB4Crunched => EtcDecoder.DecompressETC<TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.EAC_R => EtcDecoder.DecompressEACRUnsigned<TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.EAC_R_SIGNED => EtcDecoder.DecompressEACRSigned<TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.EAC_RG => EtcDecoder.DecompressEACRGUnsigned<TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.EAC_RG_SIGNED => EtcDecoder.DecompressEACRGSigned<TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.ETC2_RGB => EtcDecoder.DecompressETC2<TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.ETC2_RGBA1 => EtcDecoder.DecompressETC2A1<TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.ETC2_RGBA8 or TextureFormat.ETC_RGBA8_3DS or TextureFormat.ETC2_RGBA8Crunched => EtcDecoder.DecompressETC2A8<TColor, TChannelValue>(inputSpan, width, height, outputSpan),

			//PVRTC
			TextureFormat.PVRTC_RGB2 or TextureFormat.PVRTC_RGBA2 => PvrtcDecoder.DecompressPVRTC<TColor, TChannelValue>(inputSpan, width, height, true, outputSpan),
			TextureFormat.PVRTC_RGB4 or TextureFormat.PVRTC_RGBA4 => PvrtcDecoder.DecompressPVRTC<TColor, TChannelValue>(inputSpan, width, height, false, outputSpan),

			//YUY2
			TextureFormat.YUY2 => Yuy2Decoder.DecompressYUY2<TColor, TChannelValue>(inputSpan, width, height, outputSpan),

			//RGB
			TextureFormat.Alpha8 => RgbConverter.Convert<ColorA<byte>, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.ARGB4444 => RgbConverter.Convert<ColorARGB16, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RGBA4444 => RgbConverter.Convert<ColorRGBA16, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RGB565 => RgbConverter.Convert<ColorRGB16, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.R8 => RgbConverter.Convert<ColorR<byte>, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RG16 => RgbConverter.Convert<ColorRG<byte>, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RGB24 => RgbConverter.Convert<ColorRGB<byte>, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RGBA32 => RgbConverter.Convert<ColorRGBA<byte>, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.ARGB32 => RgbConverter.Convert<ColorARGB<byte>, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.ARGBFloat => RgbConverter.Convert<ColorARGB<float>, float, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.BGR24 => RgbConverter.Convert<ColorBGR<byte>, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.BGRA32_14 or TextureFormat.BGRA32_37 => RgbConverter.Convert<ColorBGRA<byte>, byte, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.R16 => RgbConverter.Convert<ColorR<ushort>, ushort, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RG32 => RgbConverter.Convert<ColorRG<ushort>, ushort, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RGB48 => RgbConverter.Convert<ColorRGB<ushort>, ushort, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RGBA64 => RgbConverter.Convert<ColorRGBA<ushort>, ushort, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RHalf => RgbConverter.Convert<ColorR<Half>, Half, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RGHalf => RgbConverter.Convert<ColorRG<Half>, Half, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RGBAHalf => RgbConverter.Convert<ColorRGBA<Half>, Half, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RFloat => RgbConverter.Convert<ColorR<float>, float, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RGFloat => RgbConverter.Convert<ColorRG<float>, float, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RGBAFloat => RgbConverter.Convert<ColorRGBA<float>, float, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RGB9e5Float => RgbConverter.Convert<ColorRGB9e5, double, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.R8_SIGNED => RgbConverter.Convert<ColorR<sbyte>, sbyte, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RG16_SIGNED => RgbConverter.Convert<ColorRG<sbyte>, sbyte, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RGB24_SIGNED => RgbConverter.Convert<ColorRGB<sbyte>, sbyte, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RGBA32_SIGNED => RgbConverter.Convert<ColorRGBA<sbyte>, sbyte, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.R16_SIGNED => RgbConverter.Convert<ColorR<short>, short, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RG32_SIGNED => RgbConverter.Convert<ColorRG<short>, short, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RGB48_SIGNED => RgbConverter.Convert<ColorRGB<short>, short, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			TextureFormat.RGBA64_SIGNED => RgbConverter.Convert<ColorRGBA<short>, short, TColor, TChannelValue>(inputSpan, width, height, outputSpan),
			_ => WarnAboutUnsupportedFormat(options.TextureFormat),
		};

		static int WarnAboutUnsupportedFormat(TextureFormat textureFormat)
		{
			Logger.Log(LogType.Error, LogCategory.Export, $"Unsupported texture format '{textureFormat}'");
			return -1;
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

	private static void UnpackNormal(DirectBitmap bitmap)
	{
		if (bitmap is DirectBitmap<ColorRGBA<byte>, byte> rgbaBitmap)
		{
			UnpackNormal(rgbaBitmap.Pixels);
		}
		else
		{
			Logger.Log(LogType.Warning, LogCategory.Export, "UnpackNormal called on unsupported bitmap format. Only RGBA 32 is supported.");
		}
	}

	private static void UnpackNormal<T>(Span<T> pixels) where T : unmanaged, IColor<byte>
	{
		for (int i = 0; i < pixels.Length; i++)
		{
			// Alpha and red are swapped
			// Blue needs calculated
			pixels[i].GetChannels(out byte a, out byte g, out _, out byte r);

			const double MagnitudeSqr = 255 * 255;
			double vr = r * 2.0 - 255.0;
			double vg = g * 2.0 - 255.0;
			double hypotenuseSqr = double.Min(vr * vr + vg * vg, MagnitudeSqr);
			double vb = double.Sqrt(MagnitudeSqr - hypotenuseSqr);
			double bExact = (vb + 255.0) / 2.0;
			byte b = NumericConversion.Convert<double, byte>(bExact / 255.0);

			pixels[i].SetChannels(r, g, b, a);
		}
	}
}
