using AssetRipper.IO.Files.BundleFiles;

namespace AssetRipper.IO.Files.Exceptions;

public sealed class UnsupportedBundleDecompression : NotSupportedException
{
	private UnsupportedBundleDecompression(string message) : base(message) { }

	[DoesNotReturn]
	public static void ThrowLzham(string fileName)
	{
		throw new UnsupportedBundleDecompression($"Lzham decompression is not currently supported. File: {fileName}");
	}

	[DoesNotReturn]
	public static void Throw(string fileName, CompressionType compression)
	{
		throw new UnsupportedBundleDecompression($"Bundle compression '{compression}' is not supported. '{fileName}' is likely encrypted or using a custom compression algorithm.");
	}
}
