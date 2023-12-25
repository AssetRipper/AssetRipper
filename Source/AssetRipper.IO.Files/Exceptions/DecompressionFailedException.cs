using AssetRipper.IO.Files.BundleFiles;

namespace AssetRipper.IO.Files.Exceptions;

public sealed class DecompressionFailedException : Exception
{
	private DecompressionFailedException(string message) : base(message) { }

	[DoesNotReturn]
	internal static void ThrowNoBytesWritten(string fileName, CompressionType compression)
	{
		throw new DecompressionFailedException($"Could not write any bytes for '{fileName}' while decompressing {compression}. File: {fileName}");
	}

	[DoesNotReturn]
	internal static void ThrowReadMoreThanExpected(CompressionType compression, long expected, long actual)
	{
		throw new DecompressionFailedException($"Read more than expected while decompressing {compression}. Expected {expected}, but was {actual}.");
	}

	[DoesNotReturn]
	internal static void ThrowReadMoreThanExpected(string fileName, long expected, long actual)
	{
		throw new DecompressionFailedException($"Read more than expected for '{fileName}' while decompressing. Expected {expected}, but was {actual}.");
	}

	[DoesNotReturn]
	internal static void ThrowIncorrectNumberBytesWritten(string fileName, CompressionType compression, long expected, long actual)
	{
		throw new DecompressionFailedException($"Incorrect number of bytes written for '{fileName}' while decompressing {compression}. Expected {expected}, but was {actual}.");
	}

	internal static void ThrowIfUncompressedSizeIsNegative(string fileName, long uncompressedSize)
	{
		if (uncompressedSize < 0)
		{
			throw new DecompressionFailedException($"Uncompressed size cannot be negative: {uncompressedSize}. File: {fileName}");
		}
	}
}
