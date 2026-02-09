using AssetRipper.SourceGenerated.Subclasses.PackedBitVector_Int32;
using System.Runtime.InteropServices;

namespace AssetRipper.SourceGenerated.Extensions;

public static class PackedIntVectorExtensions
{
	public static bool IsSet(this PackedBitVector_Int32 packedVector) => packedVector.NumItems > 0;

	public static void CopyValuesFrom(this PackedBitVector_Int32 instance, PackedBitVector_Int32 source)
	{
		instance.NumItems = source.NumItems;
		instance.Data = source.Data.ToArray();
		instance.BitSize = source.BitSize;
	}

	public static void PackInts(this PackedBitVector_Int32 packedVector, ReadOnlySpan<int> data)
	{
		packedVector.PackUInts(MemoryMarshal.Cast<int, uint>(data));
	}

	public static void PackUInts(this PackedBitVector_Int32 packedVector, ReadOnlySpan<uint> data)
	{
		uint maxDataValue = 0;
		for (int i = 0; i < data.Length; i++)
		{
			if (maxDataValue < data[i])
			{
				maxDataValue = data[i];
			}
		}

		packedVector.NumItems = (uint)data.Length;
		packedVector.BitSize = maxDataValue == 0xFFFFFFFF ? (byte)32 : GetBitCount(maxDataValue + 1U);
		packedVector.Data = new byte[(data.Length * packedVector.BitSize + 7) / 8];

		int bitIndex = 0;
		int byteIndex = 0;
		for (int i = 0; i < data.Length; i++)
		{
			int bitOffset = 0;
			while (bitOffset < packedVector.BitSize)
			{
				packedVector.Data[byteIndex] |= unchecked((byte)(data[i] >> bitOffset << bitIndex));
				int read = Math.Min(packedVector.BitSize - bitOffset, 8 - bitIndex);
				bitIndex += read;
				bitOffset += read;
				if (bitIndex == 8)
				{
					byteIndex++;
					bitIndex = 0;
				}
			}
		}
	}

	public static void PackUShorts(this PackedBitVector_Int32 packedVector, ReadOnlySpan<ushort> data)
	{
		uint maxDataValue = 0;
		for (int i = 0; i < data.Length; i++)
		{
			if (maxDataValue < data[i])
			{
				maxDataValue = data[i];
			}
		}

		packedVector.NumItems = (uint)data.Length;
		packedVector.BitSize = maxDataValue == 0xFFFFFFFF ? (byte)32 : GetBitCount(maxDataValue + 1U);
		packedVector.Data = new byte[(data.Length * packedVector.BitSize + 7) / 8];

		int bitIndex = 0;
		int byteIndex = 0;
		for (int i = 0; i < data.Length; i++)
		{
			int bitOffset = 0;
			while (bitOffset < packedVector.BitSize)
			{
				packedVector.Data[byteIndex] |= unchecked((byte)(data[i] >> bitOffset << bitIndex));
				int read = Math.Min(packedVector.BitSize - bitOffset, 8 - bitIndex);
				bitIndex += read;
				bitOffset += read;
				if (bitIndex == 8)
				{
					byteIndex++;
					bitIndex = 0;
				}
			}
		}
	}

	public static int[] UnpackInts(this PackedBitVector_Int32 packedVector)
	{
		int bitIndex = 0;
		int byteIndex = 0;
		int[] buffer = new int[packedVector.NumItems];
		for (int i = 0; i < packedVector.NumItems; i++)
		{
			int bitOffset = 0;
			buffer[i] = 0;
			while (bitOffset < packedVector.BitSize)
			{
				buffer[i] |= packedVector.Data[byteIndex] >> bitIndex << bitOffset;
				int read = Math.Min(packedVector.BitSize - bitOffset, 8 - bitIndex);
				bitIndex += read;
				bitOffset += read;
				if (bitIndex == 8)
				{
					byteIndex++;
					bitIndex = 0;
				}
			}
			buffer[i] &= unchecked((1 << packedVector.BitSize) - 1);
		}
		return buffer;
	}

	public static uint[] UnpackUInts(this PackedBitVector_Int32 packedVector)
	{
		int bitIndex = 0;
		int byteIndex = 0;
		uint[] buffer = new uint[packedVector.NumItems];
		for (int i = 0; i < packedVector.NumItems; i++)
		{
			int bitOffset = 0;
			buffer[i] = 0;
			while (bitOffset < packedVector.BitSize)
			{
				buffer[i] |= unchecked((uint)(packedVector.Data[byteIndex] >> bitIndex << bitOffset));
				int read = Math.Min(packedVector.BitSize - bitOffset, 8 - bitIndex);
				bitIndex += read;
				bitOffset += read;
				if (bitIndex == 8)
				{
					byteIndex++;
					bitIndex = 0;
				}
			}
			buffer[i] &= (1U << packedVector.BitSize) - 1;
		}
		return buffer;
	}

	public static ushort[] UnpackUShorts(this PackedBitVector_Int32 packedVector)
	{
		int bitIndex = 0;
		int byteIndex = 0;
		ushort[] buffer = new ushort[packedVector.NumItems];
		for (int i = 0; i < packedVector.NumItems; i++)
		{
			int bitOffset = 0;
			buffer[i] = 0;
			while (bitOffset < packedVector.BitSize)
			{
				buffer[i] |= unchecked((ushort)(packedVector.Data[byteIndex] >> bitIndex << bitOffset));
				int read = Math.Min(packedVector.BitSize - bitOffset, 8 - bitIndex);
				bitIndex += read;
				bitOffset += read;
				if (bitIndex == 8)
				{
					byteIndex++;
					bitIndex = 0;
				}
			}
			buffer[i] &= unchecked((ushort)((1U << packedVector.BitSize) - 1));
		}
		return buffer;
	}

	private static byte GetBitCount(uint value)
	{
		double log = Math.Log2(value);
		return (byte)Math.Ceiling(log);
	}
}
