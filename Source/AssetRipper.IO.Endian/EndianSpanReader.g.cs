// Auto-generated code. Do not modify manually.
using System.Buffers.Binary;

namespace AssetRipper.IO.Endian;

ref partial struct EndianSpanReader
{
	private readonly ReadOnlySpan<byte> data;
	private int offset;
	private bool bigEndian;
	public readonly int Length => data.Length;
	public int Position
	{
		readonly get => offset;
		set => offset = value;
	}

	public short ReadInt16()
	{
		short result = bigEndian
			? BinaryPrimitives.ReadInt16BigEndian(data.Slice(offset))
			: BinaryPrimitives.ReadInt16LittleEndian(data.Slice(offset));
		offset += sizeof(short);
		return result;
	}

	public ushort ReadUInt16()
	{
		ushort result = bigEndian
			? BinaryPrimitives.ReadUInt16BigEndian(data.Slice(offset))
			: BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset));
		offset += sizeof(ushort);
		return result;
	}

	public int ReadInt32()
	{
		int result = bigEndian
			? BinaryPrimitives.ReadInt32BigEndian(data.Slice(offset))
			: BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset));
		offset += sizeof(int);
		return result;
	}

	public uint ReadUInt32()
	{
		uint result = bigEndian
			? BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset))
			: BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(offset));
		offset += sizeof(uint);
		return result;
	}

	public long ReadInt64()
	{
		long result = bigEndian
			? BinaryPrimitives.ReadInt64BigEndian(data.Slice(offset))
			: BinaryPrimitives.ReadInt64LittleEndian(data.Slice(offset));
		offset += sizeof(long);
		return result;
	}

	public ulong ReadUInt64()
	{
		ulong result = bigEndian
			? BinaryPrimitives.ReadUInt64BigEndian(data.Slice(offset))
			: BinaryPrimitives.ReadUInt64LittleEndian(data.Slice(offset));
		offset += sizeof(ulong);
		return result;
	}

	public Half ReadHalf()
	{
		Half result = bigEndian
			? BinaryPrimitives.ReadHalfBigEndian(data.Slice(offset))
			: BinaryPrimitives.ReadHalfLittleEndian(data.Slice(offset));
		offset += sizeof(ushort);
		return result;
	}

	public float ReadSingle()
	{
		float result = bigEndian
			? BinaryPrimitives.ReadSingleBigEndian(data.Slice(offset))
			: BinaryPrimitives.ReadSingleLittleEndian(data.Slice(offset));
		offset += sizeof(float);
		return result;
	}

	public double ReadDouble()
	{
		double result = bigEndian
			? BinaryPrimitives.ReadDoubleBigEndian(data.Slice(offset))
			: BinaryPrimitives.ReadDoubleLittleEndian(data.Slice(offset));
		offset += sizeof(double);
		return result;
	}
}
