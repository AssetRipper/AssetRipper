namespace AssetRipper.IO.Files.Exceptions;

public sealed class DecompressionFailedException : Exception
{
	private DecompressionFailedException(string message) : base(message) { }

	[DoesNotReturn]
	internal static void ThrowReadMoreThanExpected(long expected, long actual)
	{
		throw new DecompressionFailedException($"Read more than expected. Expected {expected}, but was {actual}.");
	}

	[DoesNotReturn]
	internal static void ThrowIncorrectNumberBytesWritten(string fileName, long expected, long actual)
	{
		throw new DecompressionFailedException($"Incorrect number of bytes written for '{fileName}' while decompressing. Expected {expected}, but was {actual}.");
	}
}
