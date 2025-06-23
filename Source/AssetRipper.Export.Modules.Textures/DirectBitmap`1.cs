using AssetRipper.Conversions.FastPng;
using AssetRipper.TextureDecoder.Exr;
using AssetRipper.TextureDecoder.Rgb;
using AssetRipper.TextureDecoder.Rgb.Formats;
using StbImageWriteSharp;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AssetRipper.Export.Modules.Textures;

public sealed class DirectBitmap<TColor, TChannel> : DirectBitmap
	where TChannel : unmanaged
	where TColor : unmanaged, IColor<TChannel>
{
	private static bool UseFastBmp => true;
	private static readonly ImageWriter imageWriter = new();

	public override int PixelSize => Unsafe.SizeOf<TColor>();
	public Span<TColor> Pixels => MemoryMarshal.Cast<byte, TColor>(Bits);

	public DirectBitmap(int width, int height, int depth = 1) : base(width, height, depth)
	{
	}

	public DirectBitmap(int width, int height, int depth, byte[] data) : base(width, height, depth, data)
	{
	}

	public override DirectBitmap<TColor, TChannel> GetLayer(int layer)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(layer);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(layer, Depth);

		int layerSize = Width * Height;
		byte[] layerData = new byte[layerSize * PixelSize];
		Buffer.BlockCopy(Data, layer * layerSize * PixelSize, layerData, 0, layerSize * PixelSize);

		return new DirectBitmap<TColor, TChannel>(Width, Height, 1, layerData);
	}

	public override void FlipX()
	{
		int totalRows = Height * Depth;
		Span<TColor> pixels = Pixels;
		for (int row = 0; row < totalRows; row += Height)
		{
			Span<TColor> pixelsRow = pixels.Slice(row * Width, Width);
			pixelsRow.Reverse();
		}
	}

	public override void FlipY()
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

	public override void Transpose()
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
				int second = offset + ci * Width + ri;
				(pixels[first], pixels[second]) = (pixels[second], pixels[first]);
			}
		}
	}

	public override DirectBitmap<TColor, TChannel> DeepClone()
	{
		byte[] data = new byte[Data.Length];
		Buffer.BlockCopy(Data, 0, data, 0, Data.Length);
		return new DirectBitmap<TColor, TChannel>(Width, Height, Depth, data);
	}

	public override void SaveAsBmp(Stream stream)
	{
		if (UseFastBmp)
		{
			BmpWriter.WriteBmp(Data, Width, Height * Depth, stream);
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

	public override void SaveAsExr(Stream stream)
	{
		ExrWriter.Write<TColor, TChannel>(stream, Width, Height * Depth, Pixels);
	}

	public override void SaveAsHdr(Stream stream)
	{
		GetDataAndComponentsForSaving(out byte[] data, out ColorComponents components);
		lock (imageWriter)
		{
			imageWriter.WriteHdr(data, Width, Height * Depth, components, stream);
		}
	}

	public override void SaveAsJpeg(Stream stream)
	{
		GetDataAndComponentsForSaving(out byte[] data, out ColorComponents components);
		lock (imageWriter)
		{
			imageWriter.WriteJpg(data, Width, Height * Depth, components, stream, default);
		}
	}

	public override void SaveAsPng(Stream stream)
	{
		if (Width > ushort.MaxValue || Height * Depth > ushort.MaxValue)
		{
			// https://github.com/richgel999/fpng/issues/31
			GetDataAndComponentsForSaving(out byte[] data, out ColorComponents components);
			lock (imageWriter)
			{
				imageWriter.WritePng(data, Width, Height * Depth, components, stream);
			}
		}
		else
		{
			byte[] data;

			if (typeof(TColor) == typeof(ColorRGBA<byte>) || typeof(TColor) == typeof(ColorRGB<byte>))
			{
				data = Data;
			}
			else if (TColor.HasAlphaChannel)
			{
				RgbConverter.Convert<TColor, TChannel, ColorRGBA<byte>, byte>(Bits, Width, Height * Depth, out data);
			}
			else
			{
				RgbConverter.Convert<TColor, TChannel, ColorRGB<byte>, byte>(Bits, Width, Height * Depth, out data);
			}
			byte[] result = FPng.EncodeImageToMemory(data, Width, Height * Depth);
			new MemoryStream(result).CopyTo(stream);
		}
	}

	public override void SaveAsTga(Stream stream)
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
}
