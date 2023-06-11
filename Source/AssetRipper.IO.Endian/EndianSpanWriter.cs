using AssetRipper.Primitives;

namespace AssetRipper.IO.Endian;

public partial struct EndianSpanWriter
{
	public EndianSpanWriter(Span<byte> data, EndianType type)
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

	public void Write(bool value)
	{
		Write(value ? (byte)1 : (byte)0);
	}

	public void Write(byte value)
	{
		data[offset] = value;
		offset++;
	}

	public void Write(sbyte value)
	{
		Write(unchecked((byte)value));
	}

	public void Write(char value)
	{
		Write((ushort)value);
	}

	public void Write(ReadOnlySpan<byte> value)
	{
		value.CopyTo(data.Slice(Position));
		offset += value.Length;
	}

	/// <summary>
	/// Write a <see cref="Utf8String"/> to the data.
	/// </summary>
	/// <remarks>
	/// The binary format is a 4-byte integer length, followed by length bytes.
	/// This method does not call <see cref="Align"/>.
	/// </remarks>
	public void WriteUtf8String(Utf8String value)
	{
		Write(value.Data.Length);
		Write(value.Data);
	}

	/// <summary>
	/// Write C-like, UTF8-format, zero-terminated string.
	/// </summary>
	/// <remarks>
	/// The binary format is a series of UTF8 bytes followed with a zero byte.
	/// This method does not call <see cref="Align"/>.
	/// </remarks>
	public void WriteNullTerminatedString(Utf8String value)
	{
		Write(value.Data);
		Write((byte)'\0');
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
