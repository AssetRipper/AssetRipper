using AssetRipper.Core.IO.Asset;
using System;
using System.Linq;

namespace AssetRipper.Core.Math.PackedBitVectors
{
	public interface IPackedIntVector : IAsset
	{
		uint NumItems { get; set; }
		byte[] Data { get; set; }
		/// <summary>
		/// Maximum of 32
		/// </summary>
		byte BitSize { get; set; }
	}

	public static class PackedIntVectorExtensions
	{
		public static bool IsSet(this IPackedIntVector packedVector) => packedVector.NumItems > 0;

		public static void CopyValuesFrom(this IPackedIntVector instance, IPackedIntVector source)
		{
			instance.NumItems = source.NumItems;
			instance.Data = source.Data.ToArray();
			instance.BitSize = source.BitSize;
		}

		public static void PackUInts(this IPackedIntVector packedVector, uint[] data)
		{
			uint maxDataValue = 0;
			for (int i = 0; i < data.Length; i++)
				if (maxDataValue < data[i])
					maxDataValue = data[i];

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
					packedVector.Data[byteIndex] |= unchecked((byte)((data[i] >> bitOffset) << bitIndex));
					int read = System.Math.Min(packedVector.BitSize - bitOffset, 8 - bitIndex);
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

		public static void PackUShorts(this IPackedIntVector packedVector, ushort[] data)
		{
			uint maxDataValue = 0;
			for (int i = 0; i < data.Length; i++)
				if (maxDataValue < data[i])
					maxDataValue = data[i];

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
					packedVector.Data[byteIndex] |= unchecked((byte)((data[i] >> bitOffset) << bitIndex));
					int read = System.Math.Min(packedVector.BitSize - bitOffset, 8 - bitIndex);
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

		public static int[] UnpackInts(this IPackedIntVector packedVector)
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
					int read = System.Math.Min(packedVector.BitSize - bitOffset, 8 - bitIndex);
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

		public static uint[] UnpackUInts(this IPackedIntVector packedVector)
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
					buffer[i] |= unchecked((uint)((packedVector.Data[byteIndex] >> bitIndex) << bitOffset));
					int read = System.Math.Min(packedVector.BitSize - bitOffset, 8 - bitIndex);
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

		public static ushort[] UnpackUShorts(this IPackedIntVector packedVector)
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
					buffer[i] |= unchecked((ushort)((packedVector.Data[byteIndex] >> bitIndex) << bitOffset));
					int read = System.Math.Min(packedVector.BitSize - bitOffset, 8 - bitIndex);
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
			double log = System.Math.Log2(value);
			return (byte)System.Math.Ceiling(log);
		}
	}
}
