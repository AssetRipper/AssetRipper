namespace AssetRipper.IO.Endian.Tests;

public partial class EndianSpanTests
{
	[Test]
	public void Bytes()
	{
		const int Length = 64;
		ReadOnlySpan<byte> sourceSpan = RandomData.NextBytes(Length);
		Span<byte> targetSpan = new byte[Length];

		EndianSpanWriter writer = new EndianSpanWriter(targetSpan, EndianType.LittleEndian);
		writer.Write(sourceSpan);
		Assert.That(writer.Position, Is.EqualTo(Length));

		EndianSpanReader reader = new EndianSpanReader(targetSpan, EndianType.LittleEndian);
		ReadOnlySpan<byte> readSpan = reader.ReadBytesExact(Length);
		Assert.That(reader.Position, Is.EqualTo(Length));
		Assert.That(readSpan.SequenceEqual(sourceSpan));
	}
}
