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
}
