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

	[Theory]
	public void Utf8StringTest(EndianType endianType)
	{
		const string Content = "Ascii Characters";
		int length = sizeof(int) + Content.Length;
		byte[] data = new byte[length];
		Utf8String value1 = Content;

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(length));
		writer.WriteUtf8String(value1);
		Assert.That(writer.Position, Is.EqualTo(length));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(length));
		Utf8String value2 = reader.ReadUtf8String();
		Assert.That(reader.Position, Is.EqualTo(length));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Theory]
	public void Utf8StringEmptyTest(EndianType endianType)
	{
		int length = sizeof(int);
		byte[] data = new byte[length];
		Utf8String value1 = Utf8String.Empty;

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(length));
		writer.WriteUtf8String(value1);
		Assert.That(writer.Position, Is.EqualTo(length));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(length));
		Utf8String value2 = reader.ReadUtf8String();
		Assert.That(reader.Position, Is.EqualTo(length));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Theory]
	public void NullTerminatedStringTest(EndianType endianType)
	{
		const string Content = "Ascii Characters";
		const int Offset = 5;//Arbitrary offset so that the string isn't positioned at index 0.
		int length = sizeof(byte) + Content.Length + Offset;
		byte[] data = new byte[length];
		Utf8String value1 = Content;

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(length));
		writer.Position = Offset;
		writer.WriteNullTerminatedString(value1);
		Assert.That(writer.Position, Is.EqualTo(length));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(length));
		reader.Position = Offset;
		Utf8String value2 = reader.ReadNullTerminatedString();
		Assert.That(reader.Position, Is.EqualTo(length));
		Assert.That(value2, Is.EqualTo(value1));
	}
}
