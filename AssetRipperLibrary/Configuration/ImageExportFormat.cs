using System;
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
			return _this switch
			{
				ImageExportFormat.Bmp => ImageFormat.Bmp,
				ImageExportFormat.Gif => ImageFormat.Gif,
				ImageExportFormat.Jpeg => ImageFormat.Jpeg,
				ImageExportFormat.Png => ImageFormat.Png,
				_ => throw new ArgumentOutOfRangeException(nameof(_this)),
			};
		}

		public static string GetFileExtension(this ImageExportFormat _this)
		{
			return _this switch
			{
				ImageExportFormat.Bmp => "bmp",
				ImageExportFormat.Gif => "gif",
				ImageExportFormat.Jpeg => "jpeg",
				ImageExportFormat.Png => "png",
				_ => throw new ArgumentOutOfRangeException(nameof(_this)),
			};
		}
	}
}
