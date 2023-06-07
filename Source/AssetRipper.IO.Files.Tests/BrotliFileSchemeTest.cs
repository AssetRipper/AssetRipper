using AssetRipper.IO.Files.CompressedFiles.Brotli;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.Tests;

public class BrotliFileSchemeTests
{
	[Test]
	public void CanRead_ReturnsFalse_OnEmptyStream()
	{
		SmartStream stream = SmartStream.CreateMemory(Array.Empty<byte>());
		BrotliFileScheme scheme = new();

		bool result = scheme.CanRead(stream);

		Assert.That(result, Is.False);
	}

	[Test]
	public void CanRead_ReturnsFalse_OnNonBrotliFile()
	{
		SmartStream stream = SmartStream.CreateMemory(new byte[32]);
		BrotliFileScheme scheme = new();

		bool result = scheme.CanRead(stream);

		Assert.That(result, Is.False);
	}
}
