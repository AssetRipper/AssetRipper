using System.Drawing.Imaging;

namespace AssetRipper.Library.Configuration
{
	public enum ImageExportFormat
	{
		Bmp,
		Emf,
		Exif,
		Gif,
		Icon,
		Jpeg,
		MemoryBmp,
		Png,
		Tiff,
		Wmf,
	}

	public static class ImageExportFormatExtensions
	{
		public static ImageFormat GetImageFormat(this ImageExportFormat _this)
		{
			switch (_this)
			{
				case ImageExportFormat.Bmp:
					return ImageFormat.Bmp;
				case ImageExportFormat.Emf:
					return ImageFormat.Emf;
				case ImageExportFormat.Exif:
					return ImageFormat.Exif;
				case ImageExportFormat.Gif:
					return ImageFormat.Gif;
				case ImageExportFormat.Icon:
					return ImageFormat.Icon;
				case ImageExportFormat.Jpeg:
					return ImageFormat.Jpeg;
				case ImageExportFormat.MemoryBmp:
					return ImageFormat.MemoryBmp;
				case ImageExportFormat.Png:
					return ImageFormat.Png;
				case ImageExportFormat.Tiff:
					return ImageFormat.Tiff;
				case ImageExportFormat.Wmf:
					return ImageFormat.Wmf;
				default:
					return ImageFormat.Png;
			}
		}

		public static string GetFileExtension(this ImageExportFormat _this)
		{
			switch (_this)
			{
				case ImageExportFormat.Bmp:
					return "bmp";
				case ImageExportFormat.Emf:
					return "emf";
				case ImageExportFormat.Exif:
					return "exif";
				case ImageExportFormat.Gif:
					return "gif";
				case ImageExportFormat.Icon:
					return "icon";
				case ImageExportFormat.Jpeg:
					return "jpeg";
				case ImageExportFormat.MemoryBmp:
					return "memorybmp";
				case ImageExportFormat.Png:
					return "png";
				case ImageExportFormat.Tiff:
					return "tiff";
				case ImageExportFormat.Wmf:
					return "wmf";
				default:
					return "png";
			}
		}
	}
}
