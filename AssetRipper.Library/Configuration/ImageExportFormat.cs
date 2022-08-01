namespace AssetRipper.Library.Configuration
{
	public enum ImageExportFormat
	{
		/// <summary>
		/// Lossless. Bitmap<br/>
		/// <see href="https://en.wikipedia.org/wiki/BMP_file_format"/>
		/// </summary>
		Bmp,
		/// <summary>
		/// Lossy. Graphics Interchange Format<br/>
		/// <see href="https://en.wikipedia.org/wiki/GIF"/>
		/// </summary>
		Gif,
		/// <summary>
		/// Lossy. Joint Photographic Experts Group<br/>
		/// <see href="https://en.wikipedia.org/wiki/JPEG"/>
		/// </summary>
		Jpeg,
		/// <summary>
		/// Lossless. Portable Bitmap<br/>
		/// <see href="https://en.wikipedia.org/wiki/Netpbm"/>
		/// </summary>
		Pbm,
		/// <summary>
		/// Lossless. Portable Network Graphics<br/>
		/// <see href="https://en.wikipedia.org/wiki/Portable_Network_Graphics"/>
		/// </summary>
		Png,
		/// <summary>
		/// Lossless. Truevision TGA<br/>
		/// <see href="https://en.wikipedia.org/wiki/Truevision_TGA"/>
		/// </summary>
		Tga,
		/// <summary>
		/// Lossless. Tag Image File Format<br/>
		/// <see href="https://en.wikipedia.org/wiki/TIFF"/>
		/// </summary>
		Tiff,
		/// <summary>
		/// Lossy. Google's WebP format<br/>
		/// <see href="https://en.wikipedia.org/wiki/WebP"/>
		/// </summary>
		Webp,
	}

	public static class ImageExportFormatExtensions
	{
		public static string GetFileExtension(this ImageExportFormat _this)
		{
			return _this switch
			{
				ImageExportFormat.Bmp => "bmp",
				ImageExportFormat.Gif => "gif",
				ImageExportFormat.Jpeg => "jpeg",
				ImageExportFormat.Pbm => "pbm",
				ImageExportFormat.Png => "png",
				ImageExportFormat.Tga => "tga",
				ImageExportFormat.Tiff => "tiff",
				ImageExportFormat.Webp => "webp",
				_ => throw new ArgumentOutOfRangeException(nameof(_this)),
			};
		}
	}
}
