using AssetRipper.Core.IO.Asset;
using System;
using System.Linq;

namespace AssetRipper.Core.Math.PackedBitVectors
{
	public interface IPackedIntVector : IAsset
	{
		uint NumItems { get; set; }
		byte[] Data { get; set; }
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

		public static void Pack(this IPackedIntVector packedVector, int[] values)
		{
			throw new NotImplementedException();
		}

		public static int[] Unpack(this IPackedIntVector packedVector)
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
				buffer[i] &= (1 << packedVector.BitSize) - 1;
			}
			return buffer;
		}
	}
}
