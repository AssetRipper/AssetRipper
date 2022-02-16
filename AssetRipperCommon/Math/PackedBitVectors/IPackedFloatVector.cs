using AssetRipper.Core.IO.Asset;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.Math.PackedBitVectors
{
	public interface IPackedFloatVector : IAsset
	{
		uint NumItems { get; set; }
		float Range { get; set; }
		float Start { get; set; }
		byte[] Data { get; set; }
		byte BitSize { get; set; }
	}

	public static class PackedFloatVectorExtensions
	{
		public static bool IsSet(this IPackedFloatVector packedVector) => packedVector.NumItems > 0;

		public static void CopyValuesFrom(this IPackedFloatVector instance, IPackedFloatVector source)
		{
			instance.NumItems = source.NumItems;
			instance.Range = source.Range;
			instance.Start = source.Start;
			instance.Data = source.Data.ToArray();
			instance.BitSize = source.BitSize;
		}

		public static void Pack(this IPackedFloatVector packedVector, float[] values)
		{
			throw new NotImplementedException();
		}

		public static float[] Unpack(this IPackedFloatVector packedVector)
		{
			return packedVector.Unpack(packedVector.NumItems, 0);
		}

		public static float[] Unpack(this IPackedFloatVector packedVector, uint chunkCount, int offset)
		{
			int bitIndex = packedVector.BitSize * offset % 8;
			int byteIndex = packedVector.BitSize * offset / 8;

			float scale = 1.0f / packedVector.Range;
			float halfMaxValue = scale * ((1 << packedVector.BitSize) - 1);
			float[] buffer = new float[chunkCount];

			for (int i = 0; i < chunkCount; i++)
			{
				int value = 0;
				int bits = 0;
				while (bits < packedVector.BitSize)
				{
					value |= packedVector.Data[byteIndex] >> bitIndex << bits;
					int num = System.Math.Min(packedVector.BitSize - bits, 8 - bitIndex);
					bitIndex += num;
					bits += num;
					if (bitIndex == 8)
					{
						byteIndex++;
						bitIndex = 0;
					}
				}
				value &= (1 << packedVector.BitSize) - 1;
				buffer[i] = packedVector.Start + value / halfMaxValue;
			}
			return buffer;
		}

		public static float[] Unpack(this IPackedFloatVector packedVector, uint chunkSize, uint chunkCount, int offset)
		{
			return packedVector.Unpack(chunkSize * chunkCount, offset);
		}

		public static float[] UnpackFloats(this IPackedFloatVector packedVector, int itemCountInChunk, int chunkStride, int start = 0, int numChunks = -1)
		{
			int bitPos = packedVector.BitSize * start;
			int indexPos = bitPos / 8;
			bitPos %= 8;

			float scale = 1.0f / packedVector.Range;
			if (numChunks == -1)
				numChunks = (int)packedVector.NumItems / itemCountInChunk;
			var end = chunkStride * numChunks / 4;
			var data = new List<float>();
			for (var index = 0; index != end; index += chunkStride / 4)
				for (int i = 0; i < itemCountInChunk; ++i)
				{
					uint x = 0;

					int bits = 0;
					while (bits < packedVector.BitSize)
					{
						x |= unchecked((uint)(packedVector.Data[indexPos] >> bitPos << bits));
						int num = System.Math.Min(packedVector.BitSize - bits, 8 - bitPos);
						bitPos += num;
						bits += num;
						if (bitPos == 8)
						{
							indexPos++;
							bitPos = 0;
						}
					}
					x &= unchecked((uint)(1 << packedVector.BitSize) - 1u);
					data.Add(x / (scale * ((1 << packedVector.BitSize) - 1)) + packedVector.Start);
				}

			return data.ToArray();
		}
	}
}
