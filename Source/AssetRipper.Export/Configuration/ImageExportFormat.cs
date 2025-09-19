namespace AssetRipper.Export.Configuration;

public enum ImageExportFormat
{
	/// <summary>
	/// Lossless. Bitmap<br/>
	/// <see href="https://en.wikipedia.org/wiki/BMP_file_format"/>
	/// </summary>
	Bmp,
	/// <summary>
	/// Lossless. OpenEXR<br/>
	/// <see href="https://en.wikipedia.org/wiki/OpenEXR"/>
	/// </summary>
	Exr,
	/// <summary>
	/// Lossless. Radiance HDR<br/>
	/// <see href="https://en.wikipedia.org/wiki/RGBE_image_format"/>
	/// </summary>
	Hdr,
	/// <summary>
	/// Lossy. Joint Photographic Experts Group<br/>
	/// <see href="https://en.wikipedia.org/wiki/JPEG"/>
	/// </summary>
	Jpeg,
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
}

public static class ImageExportFormatExtensions
{
	public static string GetFileExtension(this ImageExportFormat _this)
	{
		return _this switch
		{
			ImageExportFormat.Bmp => "bmp",
			ImageExportFormat.Exr => "exr",
			ImageExportFormat.Hdr => "hdr",
			ImageExportFormat.Jpeg => "jpeg",
			ImageExportFormat.Png => "png",
			ImageExportFormat.Tga => "tga",
			_ => throw new ArgumentOutOfRangeException(nameof(_this)),
		};
	}

	//When extension types come in C# 13, this will be more convenient to use.
	public static bool TryGetFromExtension(string extension, out ImageExportFormat format)
	{
		format = extension switch
		{
			"bmp" => ImageExportFormat.Bmp,
			"exr" => ImageExportFormat.Exr,
			"hdr" => ImageExportFormat.Hdr,
			"jpeg" => ImageExportFormat.Jpeg,
			"jpg" => ImageExportFormat.Jpeg,
			"png" => ImageExportFormat.Png,
			"tga" => ImageExportFormat.Tga,
			_ => (ImageExportFormat)(-1),
		};
		return format >= 0;
	}
}
