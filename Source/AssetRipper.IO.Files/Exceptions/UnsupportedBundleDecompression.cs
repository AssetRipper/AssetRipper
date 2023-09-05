using AssetRipper.IO.Files.BundleFiles;

namespace AssetRipper.IO.Files.Exceptions;

public sealed class UnsupportedBundleDecompression : NotSupportedException
{
	private UnsupportedBundleDecompression(string message) : base(message) { }

	[DoesNotReturn]
	public static void Throw(string fileName, CompressionType compression)
	{
		throw compression switch
		{
			CompressionType.Lzham => new UnsupportedBundleDecompression($"Lzham decompression is not currently supported. File: {fileName}"),
			_ => new UnsupportedBundleDecompression($"Bundle compression '{compression}' is not supported. '{fileName}' is likely encrypted or using a custom compression algorithm."),
		};
	}
}
