using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace AssetRipper.Library.Utils
{
	public partial class DirectBitmap
	{
		private bool SaveAsBmp(Stream stream)
		{
			BmpWriter.WriteBmp(Bits, Width, Height, stream);
			return true;
		}

		private async Task SaveAsBmpAsync(Stream stream)
		{
			await Task.Run(() => BmpWriter.WriteBmp(Bits, Width, Height, stream));
		}

		private bool SaveAsGif(Stream stream)
		{
			if (OperatingSystem.IsWindows())
			{
				using Bitmap bitmap = new Bitmap(Width, Height, Stride, PixelFormat.Format32bppArgb, m_bitsHandle.AddrOfPinnedObject());
				bitmap.Save(stream, ImageFormat.Gif);
				return true;
			}
			else
			{
				using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
				image.SaveAsGif(stream);
				return true;
			}
		}

		private async Task SaveAsGifAsync(Stream stream)
		{
			using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
			await image.SaveAsGifAsync(stream);
		}

		private bool SaveAsJpeg(Stream stream)
		{
			if (OperatingSystem.IsWindows())
			{
				using Bitmap bitmap = new Bitmap(Width, Height, Stride, PixelFormat.Format32bppArgb, m_bitsHandle.AddrOfPinnedObject());
				bitmap.Save(stream, ImageFormat.Jpeg);
				return true;
			}
			else
			{
				using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
				image.SaveAsJpeg(stream);
				return true;
			}
		}

		private async Task SaveAsJpegAsync(Stream stream)
		{
			using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
			await image.SaveAsJpegAsync(stream);
		}

		private bool SaveAsPbm(Stream stream)
		{
			using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
			image.SaveAsPbm(stream);
			return true;
		}

		private async Task SaveAsPbmAsync(Stream stream)
		{
			using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
			await image.SaveAsPbmAsync(stream);
		}

		private bool SaveAsPng(Stream stream)
		{
			if (OperatingSystem.IsWindows())
			{
				using Bitmap bitmap = new Bitmap(Width, Height, Stride, PixelFormat.Format32bppArgb, m_bitsHandle.AddrOfPinnedObject());
				bitmap.Save(stream, ImageFormat.Png);
				return true;
			}
			else
			{
				using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
				image.SaveAsPng(stream);
				return true;
			}
		}

		private async Task SaveAsPngAsync(Stream stream)
		{
			using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
			await image.SaveAsPngAsync(stream);
		}

		private bool SaveAsTga(Stream stream)
		{
			using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
			image.SaveAsTga(stream);
			return true;
		}

		private async Task SaveAsTgaAsync(Stream stream)
		{
			using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
			await image.SaveAsTgaAsync(stream);
		}

		private bool SaveAsTiff(Stream stream)
		{
			if (OperatingSystem.IsWindows())
			{
				using Bitmap bitmap = new Bitmap(Width, Height, Stride, PixelFormat.Format32bppArgb, m_bitsHandle.AddrOfPinnedObject());
				bitmap.Save(stream, ImageFormat.Tiff);
				return true;
			}
			else
			{
				using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
				image.SaveAsTiff(stream);
				return true;
			}
		}

		private async Task SaveAsTiffAsync(Stream stream)
		{
			using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
			await image.SaveAsTiffAsync(stream);
		}

		private bool SaveAsWebp(Stream stream)
		{
			using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
			image.SaveAsWebp(stream);
			return true;
		}

		private async Task SaveAsWebpAsync(Stream stream)
		{
			using Image<Bgra32> image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height);
			await image.SaveAsWebpAsync(stream);
		}
	}
}
