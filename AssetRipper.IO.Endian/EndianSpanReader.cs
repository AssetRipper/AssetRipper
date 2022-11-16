namespace AssetRipper.IO.Endian;

public partial struct EndianSpanReader
{
	public EndianSpanReader(ReadOnlySpan<byte> data, EndianType type)
	{
		offset = 0;
		this.data = data;
		bigEndian = type == EndianType.BigEndian;
	}

	public EndianType Type
	{
		get => bigEndian ? EndianType.BigEndian : EndianType.LittleEndian;
		set => bigEndian = value == EndianType.BigEndian;
	}

	public bool ReadBoolean()
	{
		return ReadByte() != 0;
	}

	public byte ReadByte()
	{
		return data[offset++];
	}

	public sbyte ReadSByte()
	{
		return unchecked((sbyte)ReadByte());
	}

	public char ReadChar()
	{
		return (char)ReadUInt16();
	}
}
