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

	public byte[] ReadBytes(int count)
	{
		ThrowIfNegative(count);

		int resultLength = Math.Min(count, Length - Position);
		if (resultLength == 0)
		{
			return Array.Empty<byte>();
		}

		byte[] result = new byte[resultLength];
		data.Slice(Position, resultLength).CopyTo(result);
		Position += resultLength;
		return result;
	}

	public int ReadBytes(Span<byte> buffer)
	{
		int count = Math.Min(buffer.Length, Length - Position);
		data.Slice(Position, count).CopyTo(buffer);
		Position += count;
		return count;
	}

	public byte[] ReadBytesExact(int count)
	{
		ThrowIfNegative(count);

		if (count == 0)
		{
			return Array.Empty<byte>();
		}

		ReadOnlySpan<byte> sliced = data.Slice(Position, count);
		byte[] result = new byte[count];
		sliced.CopyTo(result);
		Position += count;
		return result;
	}

	public void ReadBytesExact(Span<byte> buffer)
	{
		data.Slice(Position, buffer.Length).CopyTo(buffer);
		Position += buffer.Length;
	}

	private static void ThrowIfNegative(int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(count), "Value cannot be negative.");
		}
	}
}
