using AssetRipper.IO.Files.Streams;

namespace AssetRipper.IO.Files.Tests;

public class PartialStreamTests
{
	[Test]
	public void PartialStreamReadByteStopsAtItsEnd()
	{
		using MemoryStream baseStream = new([1, 2, 3, 4]);
		baseStream.Position = 1;
		using PartialStream partialStream = new(baseStream, 1, 2);

		Assert.That(partialStream.ReadByte(), Is.EqualTo(2));
		Assert.That(partialStream.ReadByte(), Is.EqualTo(3));
		Assert.That(partialStream.ReadByte(), Is.EqualTo(-1));
	}

	[Test]
	public void PartialStreamCannotSeekBeforeItsStart()
	{
		using MemoryStream baseStream = new([1, 2, 3, 4]);
		using PartialStream partialStream = new(baseStream, 1, 2);

		Assert.Throws<ArgumentOutOfRangeException>(() => partialStream.Seek(-1, SeekOrigin.Begin));
		Assert.Throws<ArgumentOutOfRangeException>(() => partialStream.Seek(-3, SeekOrigin.End));
	}
}
