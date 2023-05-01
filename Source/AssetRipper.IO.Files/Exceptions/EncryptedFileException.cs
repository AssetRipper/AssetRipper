namespace AssetRipper.IO.Files.Exceptions;

public sealed class EncryptedFileException : Exception
{
	private EncryptedFileException(string fileName) : base(MakeMessage(fileName))
	{
	}

	private static string MakeMessage(string fileName)
	{
		return $"File '{fileName}' is likely encrypted or using custom compression.";
	}

	[DoesNotReturn]
	internal static void Throw(string fileName)
	{
		throw new EncryptedFileException(fileName);
	}
}
