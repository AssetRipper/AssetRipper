using AssetRipper.IO.Files.Streams;

namespace AssetRipper.IO.Files.Tests;

public class PartialStreamTests
{
	[Test]
	public async Task PartialStreamReadByteStopsAtItsEnd()
	{
		using MemoryStream baseStream = new([1, 2, 3, 4]);
		baseStream.Position = 1;
		using PartialStream partialStream = new(baseStream, 1, 2);

		await Assert.That(partialStream.ReadByte()).IsEqualTo(2);
		await Assert.That(partialStream.ReadByte()).IsEqualTo(3);
		await Assert.That(partialStream.ReadByte()).IsEqualTo(-1);
	}

	[Test]
	public async Task PartialStreamCannotSeekBeforeItsStart()
	{
		using MemoryStream baseStream = new([1, 2, 3, 4]);
		using PartialStream partialStream = new(baseStream, 1, 2);

		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Task.Run(() => partialStream.Seek(-1, SeekOrigin.Begin)));
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Task.Run(() => partialStream.Seek(-3, SeekOrigin.End)));
	}
}