using AssetRipper.Export.Configuration;

namespace AssetRipper.Export.Modules.Textures;

public abstract class DirectBitmap
{
	public static DirectBitmap Empty => EmptyDirectBitmap.Instance;

	public int Height { get; }
	public int Width { get; }
	public int Depth { get; }
	public int ByteSize => (int)CalculateByteSize(Width, Height, Depth, PixelSize);
	public Span<byte> Bits => new Span<byte>(Data, 0, ByteSize);
	public abstract int PixelSize { get; }
	protected byte[] Data { get; }
	public bool IsEmpty => Width == 0 || Height == 0 || Depth == 0;

	protected DirectBitmap(int width, int height, int depth)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(width);
		ArgumentOutOfRangeException.ThrowIfNegative(height);
		ArgumentOutOfRangeException.ThrowIfNegative(depth);
		long byteSize = CalculateByteSize(width, height, depth, PixelSize);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(byteSize, Array.MaxLength);
		Width = width;
		Height = height;
		Depth = depth;
		Data = new byte[byteSize];
	}

	protected DirectBitmap(int width, int height, int depth, byte[] data)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(width);
		ArgumentOutOfRangeException.ThrowIfNegative(height);
		ArgumentOutOfRangeException.ThrowIfNegative(depth);
		ValidateLength(width, height, depth, data);

		Width = width;
		Height = height;
		Depth = depth;
		Data = data;

		void ValidateLength(int width, int height, int depth, byte[] data)
		{
			if (data.Length < CalculateByteSize(width, height, depth, PixelSize))
			{
				throw new ArgumentException("Data does not have enough space.", nameof(data));
			}
		}
	}

	public abstract DirectBitmap GetLayer(int layer);

	public abstract void FlipX();

	public abstract void FlipY();

	public abstract void Transpose();

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

	public abstract DirectBitmap DeepClone();

	public void Save(Stream stream, ImageExportFormat format)
	{
		switch (format)
		{
			case ImageExportFormat.Bmp:
				SaveAsBmp(stream);
				break;
			case ImageExportFormat.Exr:
				SaveAsExr(stream);
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

	public abstract void SaveAsBmp(Stream stream);

	public abstract void SaveAsExr(Stream stream);

	public abstract void SaveAsHdr(Stream stream);

	public abstract void SaveAsPng(Stream stream);

	public abstract void SaveAsJpeg(Stream stream);

	public abstract void SaveAsTga(Stream stream);

	private static long CalculateByteSize(int width, int height, int depth, int pixelSize)
	{
		return 1L * width * height * depth * pixelSize;
	}
}
