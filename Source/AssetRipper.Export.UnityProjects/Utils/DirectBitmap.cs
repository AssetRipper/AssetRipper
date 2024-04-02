using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.TextureDecoder.Exr;
using AssetRipper.TextureDecoder.Rgb;
using AssetRipper.TextureDecoder.Rgb.Formats;
using StbImageWriteSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AssetRipper.Export.UnityProjects.Utils;

public readonly record struct DirectBitmap<TColor, TChannel>
	where TChannel : unmanaged
	where TColor : unmanaged, IColor<TChannel>
{
	private static bool UseFastBmp => true;
	private static readonly ImageWriter imageWriter = new();
	public DirectBitmap(int width, int height, int depth = 1)
	{
		Width = width;
		Height = height;
		Depth = depth;
		Data = new byte[CalculateByteSize(width, height, depth)];
	}

	public DirectBitmap(int width, int height, int depth, byte[] data)
	{
		ValidateLength(width, height, depth, data);

		Width = width;
		Height = height;
		Depth = depth;
		Data = data;

		static void ValidateLength(int width, int height, int depth, byte[] data)
		{
			if (data.Length < CalculateByteSize(width, height, depth))
			{
				throw new ArgumentException("Data does not have enough space.", nameof(data));
			}
		}
	}

	public void FlipX()
	{
		int totalRows = Height * Depth;
		Span<TColor> pixels = Pixels;
		for (int row = 0; row < totalRows; row += Height)
		{
			Span<TColor> pixelsRow = pixels.Slice(row * Width, Width);
			pixelsRow.Reverse();
		}
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

	public void Rotate90()
	{
		Transpose();
		FlipX();
	}

	public void Rotate180()
	{
		FlipX();
		FlipY();
	}

	public void Rotate270()
	{
		FlipX();
		Transpose();
	}

	private void Transpose()
	{
		int layerSize = Width * Height;
		Span<TColor> pixels = Pixels;
		for (int depthIndex = 0; depthIndex < Depth; depthIndex++)
		{
			int offset = depthIndex * layerSize;
			for (int i = 0; i < layerSize; i++)
			{
				int first = offset + i;
				int ci = i % Width;
				int ri = i / Width;
				int second = offset + (ci * Width) + ri;
				(pixels[first], pixels[second]) = (pixels[second], pixels[first]);
			}
		}
	}

	public DirectBitmap<TColor, TChannel> DeepClone()
	{
		byte[] data = new byte[Data.Length];
		Buffer.BlockCopy(Data, 0, data, 0, Data.Length);
		return new DirectBitmap<TColor, TChannel>(Width, Height, Depth, data);
	}

	public void Save(Stream stream, ImageExportFormat format)
	{
		switch (format)
		{
			case ImageExportFormat.Bmp:
				SaveAsBmp(stream);
				break;
			case ImageExportFormat.Exr:
				ExrWriter.Write<TColor, TChannel>(stream, Width, Height * Depth, Pixels);
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
		if (UseFastBmp)
		{
			BmpWriter.WriteBmp(Data, Width, Height * Depth, stream);
		}
		else if (OperatingSystem.IsWindows())
		{
			SaveUsingSystemDrawing(stream, ImageFormat.Bmp);
		}
		else
		{
			GetDataAndComponentsForSaving(out byte[] data, out ColorComponents components);
			lock (imageWriter)
			{
				imageWriter.WriteBmp(data, Width, Height * Depth, components, stream);
			}
		}
	}

	public void SaveAsHdr(Stream stream)
	{
		GetDataAndComponentsForSaving(out byte[] data, out ColorComponents components);
		lock (imageWriter)
		{
			imageWriter.WriteHdr(data, Width, Height * Depth, components, stream);
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
			GetDataAndComponentsForSaving(out byte[] data, out ColorComponents components);
			lock (imageWriter)
			{
				imageWriter.WriteJpg(data, Width, Height * Depth, components, stream, default);
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
			GetDataAndComponentsForSaving(out byte[] data, out ColorComponents components);
			lock (imageWriter)
			{
				imageWriter.WritePng(data, Width, Height * Depth, components, stream);
			}
		}
	}

	public void SaveAsTga(Stream stream)
	{
		GetDataAndComponentsForSaving(out byte[] data, out ColorComponents components);
		lock (imageWriter)
		{
			imageWriter.WriteTga(data, Width, Height * Depth, components, stream);
		}
	}

	private void GetDataAndComponentsForSaving(out byte[] data, out ColorComponents components)
	{
		if (typeof(TColor) == typeof(ColorRGBA<byte>))
		{
			data = Data;
			components = ColorComponents.RedGreenBlueAlpha;
		}
		else if (typeof(TColor) == typeof(ColorRGB<byte>))
		{
			data = Data;
			components = ColorComponents.RedGreenBlue;
		}
		else if (TColor.HasAlphaChannel)
		{
			RgbConverter.Convert<TColor, TChannel, ColorRGBA<byte>, byte>(Bits, Width, Height * Depth, out data);
			components = ColorComponents.RedGreenBlueAlpha;
		}
		else
		{
			RgbConverter.Convert<TColor, TChannel, ColorRGB<byte>, byte>(Bits, Width, Height * Depth, out data);
			components = ColorComponents.RedGreenBlue;
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

		static void GetData(DirectBitmap<TColor, TChannel> @this, out byte[] data, out int pixelSize, out PixelFormat pixelFormat)
		{
			if (typeof(TColor) == typeof(ColorBGRA32))
			{
				data = @this.Data;
				pixelSize = 4;
				pixelFormat = PixelFormat.Format32bppArgb;
			}
			else
			{
				RgbConverter.Convert<TColor, TChannel, ColorBGRA32, byte>(@this.Bits, @this.Width, @this.Height * @this.Depth, out data);
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
