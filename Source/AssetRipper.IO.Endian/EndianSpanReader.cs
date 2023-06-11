using AssetRipper.Primitives;

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
		readonly get => bigEndian ? EndianType.BigEndian : EndianType.LittleEndian;
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

	public ReadOnlySpan<byte> ReadBytesExact(int count)
	{
		ThrowIfNegative(count);
		ReadOnlySpan<byte> sliced = data.Slice(Position, count);
		Position += count;
		return sliced;
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
			throw new ArgumentOutOfRangeException(nameof(count), count, "Value cannot be negative.");
		}
	}

	/// <summary>
	/// Read a <see cref="Utf8String"/> from the data.
	/// </summary>
	/// <remarks>
	/// The binary format is a 4-byte integer length, followed by length bytes.
	/// This method does not call <see cref="Align"/>.
	/// </remarks>
	/// <returns>A new <see cref="Utf8String"/> containing the text.</returns>
	public Utf8String ReadUtf8String()
	{
		int length = ReadInt32();
		ReadOnlySpan<byte> byteArray = ReadBytesExact(length);
		return new Utf8String(byteArray);
	}

	/// <summary>
	/// Read C-like, UTF8-format, zero-terminated string
	/// </summary>
	/// <remarks>
	/// The binary format is a series of UTF8 bytes followed with a zero byte.
	/// This method does not call <see cref="Align"/>.
	/// </remarks>
	/// <returns>A new <see cref="Utf8String"/> containing the text.</returns>
	public Utf8String ReadNullTerminatedString()
	{
		int length = data.Slice(Position).IndexOf((byte)'\0');
		ReadOnlySpan<byte> byteArray = ReadBytesExact(length);
		Position += 1;
		return new Utf8String(byteArray);
	}

	/// <summary>
	/// Align the <see cref="Position"/> to a next multiple of 4.
	/// </summary>
	/// <remarks>
	/// If the <see cref="Position"/> is not divisible by 4, this will move it to the next multiple of 4.
	/// </remarks>
	public void Align()
	{
		Position = (Position + 3) & ~3;
	}
}
