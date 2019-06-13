using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace uTinyRipperGUI
{
	public sealed class DirectBitmap : IDisposable
	{
		public DirectBitmap(int width, int height)
		{
			Width = width;
			Height = height;
			Bits = new byte[width * height * 4];
			m_bitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
			m_bitmap = new Bitmap(Width, Height, Stride, PixelFormat.Format32bppArgb, m_bitsHandle.AddrOfPinnedObject());
		}

		~DirectBitmap()
		{
			Dispose(false);
		}

		public void SetPixel(int x, int y, Color color)
		{
			int index = (x + (y * Width)) * 4;
			unchecked
			{
				uint value = (uint)color.ToArgb();
				Bits[index + 0] = (byte)(value >> 0);
				Bits[index + 1] = (byte)(value >> 8);
				Bits[index + 2] = (byte)(value >> 16);
				Bits[index + 3] = (byte)(value >> 24);
			}
		}

		public Color GetPixel(int x, int y)
		{
			int index = x + (y * Width);
			uint col = BitConverter.ToUInt32(Bits, index);
			return Color.FromArgb(unchecked((int)col));
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

		public void Save(string filename, ImageFormat format)
		{
			m_bitmap.Save(filename, format);
		}

		public void Save(Stream stream, ImageFormat format)
		{
			m_bitmap.Save(stream, format);
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
				m_bitmap.Dispose();
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
		private readonly Bitmap m_bitmap;
		private bool m_disposed;
	}
}
