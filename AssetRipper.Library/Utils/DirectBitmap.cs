using AssetRipper.Library.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AssetRipper.Library.Utils
{
	public sealed partial class DirectBitmap : IDisposable
	{
		public int Height { get; }
		public int Width { get; }
		public int Stride => Width * 4;
		public byte[] Bits { get; }
		public IntPtr BitsPtr => m_bitsHandle.AddrOfPinnedObject();

		private readonly GCHandle m_bitsHandle;
		private bool m_disposed;

		/// <summary>
		/// Make a new bitmap
		/// </summary>
		/// <param name="width">The width of the image</param>
		/// <param name="height">The height of the image</param>
		public DirectBitmap(int width, int height) : this(width, height, new byte[width * height * 4]) { }

		/// <summary>
		/// Make a bitmap from existing BGRA32 data
		/// </summary>
		/// <param name="width">The width of the image</param>
		/// <param name="height">The height of the image</param>
		/// <param name="bgra32Data">The image data, 4 bytes per pixel. Will get pinned</param>
		public DirectBitmap(int width, int height, byte[] bgra32Data)
		{
			if (bgra32Data is null)
			{
				throw new ArgumentNullException(nameof(bgra32Data));
			}

			if (bgra32Data.Length != width * height * 4)
			{
				throw new ArgumentException($"Invalid length: expected {width * height * 4} but was actually {bgra32Data.Length}", nameof(bgra32Data));
			}

			Width = width;
			Height = height;
			Bits = bgra32Data;
			m_bitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
		}

		public unsafe void FlipY()
		{
			uint* top = (uint*)BitsPtr;
			for (int row = 0, irow = Height - 1; row < irow; row++, irow--)
			{
				uint* bottom = (uint*)(BitsPtr + (irow * Stride));
				for (int i = 0; i < Width; i++, bottom++, top++)
				{
					uint pixel = *bottom;
					*bottom = *top;
					*top = pixel;
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

		public async Task SaveAsync(Stream stream, ImageExportFormat format)
		{
			switch (format)
			{
				case ImageExportFormat.Bmp:
					await SaveAsBmpAsync(stream);
					break;
				case ImageExportFormat.Gif:
					await SaveAsGifAsync(stream);
					break;
				case ImageExportFormat.Jpeg:
					await SaveAsJpegAsync(stream);
					break;
				case ImageExportFormat.Pbm:
					await SaveAsPbmAsync(stream);
					break;
				case ImageExportFormat.Png:
					await SaveAsPngAsync(stream);
					break;
				case ImageExportFormat.Tga:
					await SaveAsTgaAsync(stream);
					break;
				case ImageExportFormat.Tiff:
					await SaveAsTiffAsync(stream);
					break;
				case ImageExportFormat.Webp:
					await SaveAsWebpAsync(stream);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(format));
			}
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
