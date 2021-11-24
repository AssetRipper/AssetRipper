using AssetRipper.Library.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AssetRipper.Library.Utils
{
	public sealed class DirectBitmap : IDisposable
	{
		public DirectBitmap(int width, int height)
		{
			Width = width;
			Height = height;
			Bits = new byte[width * height * 4];
			m_bitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
		}

		public unsafe void FlipY()
		{
			uint* top = (uint*)BitsPtr;
			for (int row = 0, irow = Height - 1; row < irow; row++, irow--)
			{
				uint* bottom = (uint*)(BitsPtr + irow * Stride);
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
			if (OperatingSystem.IsWindows())
			{
				using Bitmap bitmap = new Bitmap(Width, Height, Stride, PixelFormat.Format32bppArgb, m_bitsHandle.AddrOfPinnedObject());
				bitmap.Save(stream, format.GetImageFormat());
				return true;
			}
			else
			{
				using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
				switch (format)
				{
					case ImageExportFormat.Bmp:
						image.SaveAsBmp(stream);
						return true;
					case ImageExportFormat.Gif:
						image.SaveAsGif(stream);
						return true;
					case ImageExportFormat.Jpeg:
						image.SaveAsJpeg(stream);
						return true;
					case ImageExportFormat.Png:
						image.SaveAsPng(stream);
						return true;
					default:
						throw new ArgumentOutOfRangeException(nameof(format));
				}
			}
		}

		public bool Save(string path, ImageExportFormat format)
		{
			using FileStream stream = File.Create(path);
			return Save(stream, format);
		}

		public async Task SaveAsync(Stream stream, ImageExportFormat format)
		{
			using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
			switch (format)
			{
				case ImageExportFormat.Bmp:
					await image.SaveAsBmpAsync(stream);
					break;
				case ImageExportFormat.Gif:
					await image.SaveAsGifAsync(stream);
					break;
				case ImageExportFormat.Jpeg:
					await image.SaveAsJpegAsync(stream);
					break;
				case ImageExportFormat.Png:
					await image.SaveAsPngAsync(stream);
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

		public int Height { get; }
		public int Width { get; }
		public int Stride => Width * 4;
		public byte[] Bits { get; }
		public IntPtr BitsPtr => m_bitsHandle.AddrOfPinnedObject();

		private readonly GCHandle m_bitsHandle;
		private bool m_disposed;
	}
}
