namespace AssetRipper.IO.Endian.Tests;

public partial class EndianReaderTests
{
	[Theory]
	public void ReadStringThrowsForNegativeLength(EndianType endianType)
	{
		const int Length = -1;
		ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
		{
			EndianReader reader = new EndianReader(new MemoryStream(), endianType);
			reader.ReadString(Length);
		});
		Assert.That(exception.ActualValue, Is.EqualTo(Length));
	}

	[Theory]
	public void ReadStringReturnsEmptyStringForLengthZero(EndianType endianType)
	{
		EndianReader reader = new EndianReader(new MemoryStream(), endianType);
		Assert.That(reader.ReadString(0), Is.EqualTo(string.Empty));
	}
}
