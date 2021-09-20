using AssetRipper.Library.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

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

		~DirectBitmap()
		{
			Dispose(false);
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
				var bitmap = new Bitmap(Width, Height, Stride, PixelFormat.Format32bppArgb, m_bitsHandle.AddrOfPinnedObject());
				bitmap.Save(stream, format.GetImageFormat());
				bitmap.Dispose();
				return true;
			}
			else
			{
				var image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
				switch (format)
				{
					case ImageExportFormat.Bmp:
						image.SaveAsBmp(stream);
						image.Dispose();
						return true;
					case ImageExportFormat.Gif:
						image.SaveAsGif(stream);
						image.Dispose();
						return true;
					case ImageExportFormat.Jpeg:
						image.SaveAsJpeg(stream);
						image.Dispose();
						return true;
					case ImageExportFormat.Png:
						image.SaveAsPng(stream);
						image.Dispose();
						return true;
					default:
						return false;
				}
			}
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

		public int Height { get; }
		public int Width { get; }
		public int Stride => Width * 4;
		public byte[] Bits { get; }
		public IntPtr BitsPtr => m_bitsHandle.AddrOfPinnedObject();

		private readonly GCHandle m_bitsHandle;
		private bool m_disposed;
	}
}
