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
	internal static void ThrowIncorrectNumberBytesWritten(long expected, long actual)
	{
		throw new DecompressionFailedException($"Incorrect number of bytes written. Expected {expected}, but was {actual}.");
	}
}
