namespace AssetRipper.IO.Endian.Tests;

public partial class EndianSpanWriterTests
{
	[Theory]
	public void EndianTypeTest(EndianType endianType)
	{
		EndianSpanWriter writer = new EndianSpanWriter(default, endianType);
		Assert.That(writer.Type, Is.EqualTo(endianType));

		EndianType otherType = endianType == EndianType.BigEndian ? EndianType.LittleEndian :EndianType.BigEndian;
		writer.Type = otherType;
		Assert.That(writer.Type, Is.EqualTo(otherType));
	}

	[Theory]
	public void AlignDoesNotThrowForZeroLength(EndianType endianType)
	{
		Assert.DoesNotThrow(() =>
		{
			EndianSpanWriter writer = new EndianSpanWriter(default, endianType);
			writer.Align();
		});
	}

	[Theory]
	public void AlignMovesPositionToMultipleOfFour(EndianType endianType)
	{
		Span<byte> data = stackalloc byte[6];
		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		for (int i = 1; i <= 4; i++)
		{
			writer.Position = i;
			writer.Align();
			Assert.That(writer.Position, Is.EqualTo(4));
		}
	}
}
