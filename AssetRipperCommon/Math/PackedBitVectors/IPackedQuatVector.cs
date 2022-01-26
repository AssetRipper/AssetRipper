using AssetRipper.Core.Math.Vectors;
using System;

namespace AssetRipper.Core.Math.PackedBitVectors
{
	public interface IPackedQuatVector
	{
		uint NumItems { get; set; }
		byte[] Data { get; set; }
	}

	public static class PackedQuatVectorExtensions
	{
		public static Quaternionf[] Unpack(this IPackedQuatVector packedVector)
		{
			int bitIndex = 0;
			int byteIndex = 0;
			Quaternionf[] buffer = new Quaternionf[packedVector.NumItems];
			for (int i = 0; i < packedVector.NumItems; i++)
			{
				int flags = 0;
				int bitOffset = 0;
				while (bitOffset < 3)
				{
					flags |= packedVector.Data[byteIndex] >> bitIndex << bitOffset;
					int read = System.Math.Min(3 - bitOffset, 8 - bitIndex);
					bitIndex += read;
					bitOffset += read;
					if (bitIndex == 8)
					{
						byteIndex++;
						bitIndex = 0;
					}
				}
				flags &= 7;

				float sum = 0;
				Quaternionf quaternion = new Quaternionf();
				for (int j = 0; j < 4; j++)
					if ((flags & 3) != j)
					{
						int bitSize = (flags + 1 & 3) == j ? 9 : 10;
						float halfMaxValue = 0.5f * ((1 << bitSize) - 1);

						int value = 0;
						bitOffset = 0;
						while (bitOffset < bitSize)
						{
							value |= packedVector.Data[byteIndex] >> bitIndex << bitOffset;
							int num = System.Math.Min(bitSize - bitOffset, 8 - bitIndex);
							bitIndex += num;
							bitOffset += num;
							if (bitIndex == 8)
							{
								byteIndex++;
								bitIndex = 0;
							}
						}
						value &= (1 << bitSize) - 1;
						// final value's range is [-1.0f : 1.0f]
						quaternion[j] = value / halfMaxValue - 1.0f;
						sum += quaternion[j] * quaternion[j];
					}

				int lastComponent = flags & 3;
				quaternion[lastComponent] = (float)System.Math.Sqrt(1.0f - sum);
				if ((flags & 4) != 0)
					quaternion[lastComponent] = -quaternion[lastComponent];
				buffer[i] = quaternion;
			}
			return buffer;
		}
	}
}
