namespace AssetRipper.IO.Endian.Tests;

public partial class EndianSpanTests
{
	[Theory]
	public void BytesExactTest(EndianType endianType)
	{
		const int Length = 64;
		ReadOnlySpan<byte> sourceSpan = RandomData.NextBytes(Length);
		Span<byte> targetSpan = new byte[Length];

		EndianSpanWriter writer = new EndianSpanWriter(targetSpan, endianType);
		Assert.That(writer.Length, Is.EqualTo(Length));
		writer.Write(sourceSpan);
		Assert.That(writer.Position, Is.EqualTo(Length));

		EndianSpanReader reader = new EndianSpanReader(targetSpan, endianType);
		Assert.That(reader.Length, Is.EqualTo(Length));
		ReadOnlySpan<byte> readSpan = reader.ReadBytesExact(Length);
		Assert.That(reader.Position, Is.EqualTo(Length));
		Assert.That(readSpan.SequenceEqual(sourceSpan));
	}

	[Theory]
	public void PositionTest(EndianType endianType)
	{
		byte[] data = new byte[2 * sizeof(int)];
		int value1 = RandomData.NextInt32();

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		writer.Position = sizeof(int);
		Assert.That(writer.Position, Is.EqualTo(sizeof(int)));
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(2 * sizeof(int)));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Position, Is.EqualTo(0));
		Assert.That(reader.ReadInt32(), Is.EqualTo(0));
		Assert.That(reader.Position, Is.EqualTo(sizeof(int)));
		int value2 = reader.ReadInt32();
		Assert.That(reader.Position, Is.EqualTo(2 * sizeof(int)));
		Assert.That(value2, Is.EqualTo(value1));

		const int newPosition = 2;
		reader.Position = newPosition;
		Assert.That(reader.Position, Is.EqualTo(newPosition));
	}
}
