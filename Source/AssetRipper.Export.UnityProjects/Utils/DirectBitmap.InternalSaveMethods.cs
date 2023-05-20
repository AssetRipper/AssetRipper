using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace AssetRipper.Export.UnityProjects.Utils
{
	public partial class DirectBitmap
	{
		public bool SaveAsBmp(Stream stream)
		{
			//BmpWriter.WriteBmp(Bits, Width, Height * Depth, stream);
			//return true;
			if (OperatingSystem.IsWindows())
			{
				using Bitmap bitmap = ToSystemBitmap();
				bitmap.Save(stream, ImageFormat.Bmp);
				return true;
			}
			else
			{
				using Image<Bgra32> image = ToImageSharp();
				image.SaveAsBmp(stream);
				return true;
			}
		}

		public async Task SaveAsBmpAsync(Stream stream)
		{
			//await Task.Run(() => BmpWriter.WriteBmp(Bits, Width, Height * Depth, stream));
			using Image<Bgra32> image = ToImageSharp();
			await image.SaveAsBmpAsync(stream);
		}

		public bool SaveAsGif(Stream stream)
		{
			if (OperatingSystem.IsWindows())
			{
				using Bitmap bitmap = ToSystemBitmap();
				bitmap.Save(stream, ImageFormat.Gif);
				return true;
			}
			else
			{
				using Image<Bgra32> image = ToImageSharp();
				image.SaveAsGif(stream);
				return true;
			}
		}

		public async Task SaveAsGifAsync(Stream stream)
		{
			using Image<Bgra32> image = ToImageSharp();
			await image.SaveAsGifAsync(stream);
		}

		public bool SaveAsJpeg(Stream stream)
		{
			if (OperatingSystem.IsWindows())
			{
				using Bitmap bitmap = ToSystemBitmap();
				bitmap.Save(stream, ImageFormat.Jpeg);
				return true;
			}
			else
			{
				using Image<Bgra32> image = ToImageSharp();
				image.SaveAsJpeg(stream);
				return true;
			}
		}

		public async Task SaveAsJpegAsync(Stream stream)
		{
			using Image<Bgra32> image = ToImageSharp();
			await image.SaveAsJpegAsync(stream);
		}

		public bool SaveAsPbm(Stream stream)
		{
			using Image<Bgra32> image = ToImageSharp();
			image.SaveAsPbm(stream);
			return true;
		}

		public async Task SaveAsPbmAsync(Stream stream)
		{
			using Image<Bgra32> image = ToImageSharp();
			await image.SaveAsPbmAsync(stream);
		}

		public bool SaveAsPng(Stream stream)
		{
			if (OperatingSystem.IsWindows())
			{
				using Bitmap bitmap = ToSystemBitmap();
				bitmap.Save(stream, ImageFormat.Png);
				return true;
			}
			else
			{
				using Image<Bgra32> image = ToImageSharp();
				image.SaveAsPng(stream);
				return true;
			}
		}

		public async Task SaveAsPngAsync(Stream stream)
		{
			using Image<Bgra32> image = ToImageSharp();
			await image.SaveAsPngAsync(stream);
		}

		public bool SaveAsTga(Stream stream)
		{
			using Image<Bgra32> image = ToImageSharp();
			image.SaveAsTga(stream);
			return true;
		}

		public async Task SaveAsTgaAsync(Stream stream)
		{
			using Image<Bgra32> image = ToImageSharp();
			await image.SaveAsTgaAsync(stream);
		}

		public bool SaveAsTiff(Stream stream)
		{
			if (OperatingSystem.IsWindows())
			{
				using Bitmap bitmap = ToSystemBitmap();
				bitmap.Save(stream, ImageFormat.Tiff);
				return true;
			}
			else
			{
				using Image<Bgra32> image = ToImageSharp();
				image.SaveAsTiff(stream);
				return true;
			}
		}

		public async Task SaveAsTiffAsync(Stream stream)
		{
			using Image<Bgra32> image = ToImageSharp();
			await image.SaveAsTiffAsync(stream);
		}

		public bool SaveAsWebp(Stream stream)
		{
			using Image<Bgra32> image = ToImageSharp();
			image.SaveAsWebp(stream);
			return true;
		}

		public async Task SaveAsWebpAsync(Stream stream)
		{
			using Image<Bgra32> image = ToImageSharp();
			await image.SaveAsWebpAsync(stream);
		}

		[SupportedOSPlatform("windows")]
		private Bitmap ToSystemBitmap()
		{
			return new Bitmap(Width, Height * Depth, Stride, PixelFormat.Format32bppArgb, m_bitsHandle.AddrOfPinnedObject());
		}

		private Image<Bgra32> ToImageSharp()
		{
			return SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(Bits, Width, Height * Depth);
		}
	}
}
