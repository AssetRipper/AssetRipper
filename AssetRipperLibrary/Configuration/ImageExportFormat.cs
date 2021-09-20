using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace AssetRipper.Library.Configuration
{
	public enum ImageExportFormat
	{
		/// <summary>
		/// Lossless. Bitmap
		/// </summary>
		Bmp,
		/// <summary>
		/// Lossy. Graphics Interchange Format
		/// </summary>
		Gif,
		/// <summary>
		/// Lossy. Joint Photographic Experts Group
		/// </summary>
		Jpeg,
		/// <summary>
		/// Lossless. Portable Network Graphics
		/// </summary>
		Png,
	}

	public static class ImageExportFormatExtensions
	{
		[SupportedOSPlatform("windows")]
		public static ImageFormat GetImageFormat(this ImageExportFormat _this)
		{
			switch (_this)
			{
				case ImageExportFormat.Bmp:
					return ImageFormat.Bmp;
				case ImageExportFormat.Gif:
					return ImageFormat.Gif;
				case ImageExportFormat.Jpeg:
					return ImageFormat.Jpeg;
				case ImageExportFormat.Png:
					return ImageFormat.Png;
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
				case ImageExportFormat.Gif:
					return "gif";
				case ImageExportFormat.Jpeg:
					return "jpeg";
				case ImageExportFormat.Png:
					return "png";
				default:
					return "png";
			}
		}
	}
}
