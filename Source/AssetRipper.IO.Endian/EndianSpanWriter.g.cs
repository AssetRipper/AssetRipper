// Auto-generated code. Do not modify manually.
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace AssetRipper.IO.Endian;

ref partial struct EndianSpanWriter
{
	private readonly Span<byte> data;
	private int offset;
	private bool bigEndian;
	public readonly int Length => data.Length;
	public int Position
	{
		readonly get => offset;
		set => offset = value;
	}

	public void Write(short value)
	{
		if (bigEndian)
		{
			BinaryPrimitives.WriteInt16BigEndian(data.Slice(offset), value);
		}
		else
		{
			BinaryPrimitives.WriteInt16LittleEndian(data.Slice(offset), value);
		}
		offset += sizeof(short);
	}

	public void Write(ushort value)
	{
		if (bigEndian)
		{
			BinaryPrimitives.WriteUInt16BigEndian(data.Slice(offset), value);
		}
		else
		{
			BinaryPrimitives.WriteUInt16LittleEndian(data.Slice(offset), value);
		}
		offset += sizeof(ushort);
	}

	public void Write(int value)
	{
		if (bigEndian)
		{
			BinaryPrimitives.WriteInt32BigEndian(data.Slice(offset), value);
		}
		else
		{
			BinaryPrimitives.WriteInt32LittleEndian(data.Slice(offset), value);
		}
		offset += sizeof(int);
	}

	public void Write(uint value)
	{
		if (bigEndian)
		{
			BinaryPrimitives.WriteUInt32BigEndian(data.Slice(offset), value);
		}
		else
		{
			BinaryPrimitives.WriteUInt32LittleEndian(data.Slice(offset), value);
		}
		offset += sizeof(uint);
	}

	public void Write(long value)
	{
		if (bigEndian)
		{
			BinaryPrimitives.WriteInt64BigEndian(data.Slice(offset), value);
		}
		else
		{
			BinaryPrimitives.WriteInt64LittleEndian(data.Slice(offset), value);
		}
		offset += sizeof(long);
	}

	public void Write(ulong value)
	{
		if (bigEndian)
		{
			BinaryPrimitives.WriteUInt64BigEndian(data.Slice(offset), value);
		}
		else
		{
			BinaryPrimitives.WriteUInt64LittleEndian(data.Slice(offset), value);
		}
		offset += sizeof(ulong);
	}

	public void Write(Half value)
	{
		if (bigEndian)
		{
			BinaryPrimitives.WriteHalfBigEndian(data.Slice(offset), value);
		}
		else
		{
			BinaryPrimitives.WriteHalfLittleEndian(data.Slice(offset), value);
		}
		offset += sizeof(ushort);
	}

	public void Write(float value)
	{
		if (bigEndian)
		{
			BinaryPrimitives.WriteSingleBigEndian(data.Slice(offset), value);
		}
		else
		{
			BinaryPrimitives.WriteSingleLittleEndian(data.Slice(offset), value);
		}
		offset += sizeof(float);
	}

	public void Write(double value)
	{
		if (bigEndian)
		{
			BinaryPrimitives.WriteDoubleBigEndian(data.Slice(offset), value);
		}
		else
		{
			BinaryPrimitives.WriteDoubleLittleEndian(data.Slice(offset), value);
		}
		offset += sizeof(double);
	}

	/// <summary>
	/// Write a C# primitive type. JIT optimizations should make this as efficient as normal method calls.
	/// </summary>
	public void WritePrimitive<T>(T value) where T : unmanaged
	{
		if (typeof(T) == typeof(short))
		{
			Write(Unsafe.As<T, short>(ref value));
		}
		else if (typeof(T) == typeof(ushort))
		{
			Write(Unsafe.As<T, ushort>(ref value));
		}
		else if (typeof(T) == typeof(int))
		{
			Write(Unsafe.As<T, int>(ref value));
		}
		else if (typeof(T) == typeof(uint))
		{
			Write(Unsafe.As<T, uint>(ref value));
		}
		else if (typeof(T) == typeof(long))
		{
			Write(Unsafe.As<T, long>(ref value));
		}
		else if (typeof(T) == typeof(ulong))
		{
			Write(Unsafe.As<T, ulong>(ref value));
		}
		else if (typeof(T) == typeof(Half))
		{
			Write(Unsafe.As<T, Half>(ref value));
		}
		else if (typeof(T) == typeof(float))
		{
			Write(Unsafe.As<T, float>(ref value));
		}
		else if (typeof(T) == typeof(double))
		{
			Write(Unsafe.As<T, double>(ref value));
		}
		else if (typeof(T) == typeof(bool))
		{
			Write(Unsafe.As<T, bool>(ref value));
		}
		else if (typeof(T) == typeof(byte))
		{
			Write(Unsafe.As<T, byte>(ref value));
		}
		else if (typeof(T) == typeof(sbyte))
		{
			Write(Unsafe.As<T, sbyte>(ref value));
		}
		else if (typeof(T) == typeof(char))
		{
			Write(Unsafe.As<T, char>(ref value));
		}
	}
}
