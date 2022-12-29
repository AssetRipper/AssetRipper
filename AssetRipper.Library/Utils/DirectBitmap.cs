using AssetRipper.Library.Configuration;
using AssetRipper.TextureDecoder.Rgb.Formats;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AssetRipper.Library.Utils
{
	public sealed partial class DirectBitmap : IDisposable
	{
		public int Width { get; }
		public int Height { get; }
		public int Depth { get; }
		public int Stride => Width * PixelSize;
		public byte[] Bits { get; }
		public IntPtr BitsPtr => m_bitsHandle.AddrOfPinnedObject();

		private readonly GCHandle m_bitsHandle;
		private bool m_disposed;
		/// <summary>
		/// The byte size of each pixel.
		/// </summary>
		public static int PixelSize => Unsafe.SizeOf<ColorBGRA32>();

		/// <summary>
		/// Make a new bitmap of BGRA32 pixels.
		/// </summary>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="depth">The depth of the image.</param>
		public DirectBitmap(int width, int height, int depth = 1)
		{
			ValidateDimensions(width, height, depth);
			Width = width;
			Height = height;
			Depth = depth;
			Bits = new byte[width * height * depth * PixelSize];
			m_bitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
		}

		/// <summary>
		/// Make a bitmap from existing BGRA32 data.
		/// </summary>
		/// <param name="imageData">The BGRA32 image data, 4 bytes per pixel. Will get pinned.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="depth">The depth of the image.</param>
		public DirectBitmap(byte[] imageData, int width, int height, int depth = 1)
		{
			ValidateImageData(imageData, width, height, depth);
			Width = width;
			Height = height;
			Depth = depth;
			Bits = imageData;
			m_bitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
		}

		private static void ValidateDimensions(int width, int height, int depth)
		{
			if (width <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(width), width, null);
			}
			if (height <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(height), height, null);
			}
			if (depth <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(depth), depth, null);
			}
		}

		private static void ValidateImageData([NotNull] byte[]? imageData, int width, int height, int depth)
		{
			ArgumentNullException.ThrowIfNull(imageData, nameof(imageData));

			if (imageData.Length != width * height * depth * PixelSize)
			{
				throw new ArgumentException($"Invalid length: expected {width * height * depth * PixelSize} but was actually {imageData.Length}", nameof(imageData));
			}
		}

		public void FlipY()
		{
			int totalRows = Height * Depth;
			Span<ColorBGRA32> pixels = MemoryMarshal.Cast<byte, ColorBGRA32>(Bits);
			for (int startingRow = 0; startingRow < totalRows; startingRow += Height)
			{
				int endingRow = startingRow + Height - 1;
				for (int row = startingRow, irow = endingRow; row < irow; row++, irow--)
				{
					Span<ColorBGRA32> rowTop = pixels.Slice(row * Width, Width);
					Span<ColorBGRA32> rowBottom = pixels.Slice(irow * Width, Width);
					for (int i = 0; i < Width; i++)
					{
						(rowTop[i], rowBottom[i]) = (rowBottom[i], rowTop[i]);
					}
				}
			}
		}

		public bool Save(Stream stream, ImageExportFormat format)
		{
			return format switch
			{
				ImageExportFormat.Bmp => SaveAsBmp(stream),
				ImageExportFormat.Gif => SaveAsGif(stream),
				ImageExportFormat.Jpeg => SaveAsJpeg(stream),
				ImageExportFormat.Pbm => SaveAsPbm(stream),
				ImageExportFormat.Png => SaveAsPng(stream),
				ImageExportFormat.Tga => SaveAsTga(stream),
				ImageExportFormat.Tiff => SaveAsTiff(stream),
				ImageExportFormat.Webp => SaveAsWebp(stream),
				_ => throw new ArgumentOutOfRangeException(nameof(format)),
			};
		}

		public bool Save(string path, ImageExportFormat format)
		{
			using FileStream stream = File.Create(path);
			return Save(stream, format);
		}

		public Task SaveAsync(Stream stream, ImageExportFormat format)
		{
			return format switch
			{
				ImageExportFormat.Bmp => SaveAsBmpAsync(stream),
				ImageExportFormat.Gif => SaveAsGifAsync(stream),
				ImageExportFormat.Jpeg => SaveAsJpegAsync(stream),
				ImageExportFormat.Pbm => SaveAsPbmAsync(stream),
				ImageExportFormat.Png => SaveAsPngAsync(stream),
				ImageExportFormat.Tga => SaveAsTgaAsync(stream),
				ImageExportFormat.Tiff => SaveAsTiffAsync(stream),
				ImageExportFormat.Webp => SaveAsWebpAsync(stream),
				_ => throw new ArgumentOutOfRangeException(nameof(format)),
			};
		}

		public async Task SaveAsync(string path, ImageExportFormat format)
		{
			using FileStream stream = File.Create(path);
			await SaveAsync(stream, format);
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		private void Dispose(bool _)
		{
			if (!m_disposed)
			{
				m_bitsHandle.Free();
				m_disposed = true;
			}
		}

		~DirectBitmap()
		{
			Dispose(false);
		}
	}
}
