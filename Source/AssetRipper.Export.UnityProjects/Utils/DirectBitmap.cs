using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.TextureDecoder.Rgb;
using AssetRipper.TextureDecoder.Rgb.Formats;
using StbImageWriteSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AssetRipper.Export.UnityProjects.Utils;

public readonly record struct DirectBitmap<TColor, TColorArg>
	where TColorArg : unmanaged
	where TColor : unmanaged, IColor<TColorArg>
{
	public DirectBitmap(int width, int height, int depth = 1)
	{
		Width = width;
		Height = height;
		Depth = depth;
		Data = new byte[CalculateByteSize(width, height, depth)];
	}

	public DirectBitmap(int width, int height, int depth, byte[] data)
	{
		if (data.Length < CalculateByteSize(width, height, depth))
		{
			throw new ArgumentException("Data does not have enough space.", nameof(data));
		}

		Width = width;
		Height = height;
		Depth = depth;
		Data = data;
	}

	public void FlipY()
	{
		int totalRows = Height * Depth;
		Span<TColor> pixels = Pixels;
		for (int startingRow = 0; startingRow < totalRows; startingRow += Height)
		{
			int endingRow = startingRow + Height - 1;
			for (int row = startingRow, irow = endingRow; row < irow; row++, irow--)
			{
				Span<TColor> rowTop = pixels.Slice(row * Width, Width);
				Span<TColor> rowBottom = pixels.Slice(irow * Width, Width);
				for (int i = 0; i < Width; i++)
				{
					(rowTop[i], rowBottom[i]) = (rowBottom[i], rowTop[i]);
				}
			}
		}
	}

	public void Save(Stream stream, ImageExportFormat format)
	{
		switch (format)
		{
			case ImageExportFormat.Bmp:
				SaveAsBmp(stream);
				break;
			case ImageExportFormat.Hdr:
				SaveAsHdr(stream);
				break;
			case ImageExportFormat.Jpeg:
				SaveAsJpeg(stream);
				break;
			case ImageExportFormat.Png:
				SaveAsPng(stream);
				break;
			case ImageExportFormat.Tga:
				SaveAsTga(stream);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(format));
		}
	}

	public void SaveAsBmp(Stream stream)
	{
		if (OperatingSystem.IsWindows())
		{
			SaveUsingSystemDrawing(stream, ImageFormat.Bmp);
		}
		else
		{
			ImageWriter writer = new();
			if (typeof(TColor) == typeof(ColorRGBA32))
			{
				writer.WriteBmp(Data, Width, Height, ColorComponents.RedGreenBlueAlpha, stream);
			}
			else if (typeof(TColor) == typeof(ColorRGB24))
			{
				writer.WriteBmp(Data, Width, Height, ColorComponents.RedGreenBlue, stream);
			}
			else
			{
				RgbConverter.Convert<TColor, TColorArg, ColorRGBA32, byte>(Bits, Width, Height, out byte[] output);
				writer.WriteBmp(output, Width, Height, ColorComponents.RedGreenBlueAlpha, stream);
			}
		}
	}

	public void SaveAsHdr(Stream stream)
	{
		ImageWriter writer = new();
		if (typeof(TColor) == typeof(ColorRGBA32))
		{
			writer.WriteHdr(Data, Width, Height, ColorComponents.RedGreenBlueAlpha, stream);
		}
		else if (typeof(TColor) == typeof(ColorRGB24))
		{
			writer.WriteHdr(Data, Width, Height, ColorComponents.RedGreenBlue, stream);
		}
		else
		{
			RgbConverter.Convert<TColor, TColorArg, ColorRGBA32, byte>(Bits, Width, Height, out byte[] output);
			writer.WriteHdr(output, Width, Height, ColorComponents.RedGreenBlueAlpha, stream);
		}
	}

	public void SaveAsJpeg(Stream stream)
	{
		if (OperatingSystem.IsWindows())
		{
			SaveUsingSystemDrawing(stream, ImageFormat.Jpeg);
		}
		else
		{
			ImageWriter writer = new();
			if (typeof(TColor) == typeof(ColorRGBA32))
			{
				writer.WriteJpg(Data, Width, Height, ColorComponents.RedGreenBlueAlpha, stream, default);
			}
			else if (typeof(TColor) == typeof(ColorRGB24))
			{
				writer.WriteJpg(Data, Width, Height, ColorComponents.RedGreenBlue, stream, default);
			}
			else
			{
				RgbConverter.Convert<TColor, TColorArg, ColorRGBA32, byte>(Bits, Width, Height, out byte[] output);
				writer.WriteJpg(output, Width, Height, ColorComponents.RedGreenBlueAlpha, stream, default);
			}
		}
	}

	public void SaveAsPng(Stream stream)
	{
		if (OperatingSystem.IsWindows())
		{
			SaveUsingSystemDrawing(stream, ImageFormat.Png);
		}
		else
		{
			ImageWriter writer = new();
			if (typeof(TColor) == typeof(ColorRGBA32))
			{
				writer.WritePng(Data, Width, Height, ColorComponents.RedGreenBlueAlpha, stream);
			}
			else if (typeof(TColor) == typeof(ColorRGB24))
			{
				writer.WritePng(Data, Width, Height, ColorComponents.RedGreenBlue, stream);
			}
			else
			{
				RgbConverter.Convert<TColor, TColorArg, ColorRGBA32, byte>(Bits, Width, Height, out byte[] output);
				writer.WritePng(output, Width, Height, ColorComponents.RedGreenBlueAlpha, stream);
			}
		}
	}

	public void SaveAsTga(Stream stream)
	{
		ImageWriter writer = new();
		if (typeof(TColor) == typeof(ColorRGBA32))
		{
			writer.WriteTga(Data, Width, Height, ColorComponents.RedGreenBlueAlpha, stream);
		}
		else if (typeof(TColor) == typeof(ColorRGB24))
		{
			writer.WriteTga(Data, Width, Height, ColorComponents.RedGreenBlue, stream);
		}
		else
		{
			RgbConverter.Convert<TColor, TColorArg, ColorRGBA32, byte>(Bits, Width, Height, out byte[] output);
			writer.WriteTga(output, Width, Height, ColorComponents.RedGreenBlueAlpha, stream);
		}
	}

	[SupportedOSPlatform("windows")]
	private void SaveUsingSystemDrawing(Stream stream, ImageFormat format)
	{
		GetData(this, out byte[] data, out int pixelSize, out PixelFormat pixelFormat);
		GCHandle bitsHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
		try
		{
			using Bitmap bitmap = new Bitmap(Width, Height * Depth, Width * pixelSize, pixelFormat, bitsHandle.AddrOfPinnedObject());
			bitmap.Save(stream, format);
		}
		finally
		{
			bitsHandle.Free();
		}

		static void GetData(DirectBitmap<TColor, TColorArg> @this, out byte[] data, out int pixelSize, out PixelFormat pixelFormat)
		{
			if (typeof(TColor) == typeof(ColorBGRA32))
			{
				data = @this.Data;
				pixelSize = 4;
				pixelFormat = PixelFormat.Format32bppArgb;
			}
			else
			{
				RgbConverter.Convert<TColor, TColorArg, ColorBGRA32, byte>(@this.Bits, @this.Width, @this.Height, out data);
				pixelSize = 4;
				pixelFormat = PixelFormat.Format32bppArgb;
			}
		}
	}

	public int Height { get; }
	public int Width { get; }
	public int Depth { get; }
	public int ByteSize => CalculateByteSize(Width, Height, Depth);
	public Span<byte> Bits => new Span<byte>(Data, 0, ByteSize);
	public Span<TColor> Pixels => MemoryMarshal.Cast<byte, TColor>(Bits);
	public static int PixelSize => Unsafe.SizeOf<TColor>();
	private byte[] Data { get; }

	private static int CalculateByteSize(int width, int height, int depth)
	{
		return width * height * depth * PixelSize;
	}
}
