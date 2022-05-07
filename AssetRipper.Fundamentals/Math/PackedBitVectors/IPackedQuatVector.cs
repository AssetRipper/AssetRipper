﻿using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using System;
using System.Linq;

namespace AssetRipper.Core.Math.PackedBitVectors
{
	public interface IPackedQuatVector : IAsset
	{
		uint NumItems { get; set; }
		byte[] Data { get; set; }
	}

	public static class PackedQuatVectorExtensions
	{
		public static void CopyValuesFrom(this IPackedQuatVector instance, IPackedQuatVector source)
		{
			instance.NumItems = source.NumItems;
			instance.Data = source.Data.ToArray();
		}

		public static void Pack(this IPackedQuatVector packedVector, IQuaternionf[] inputData)
		{
			packedVector.NumItems = (uint)inputData.Length;
			packedVector.Data = new byte[inputData.Length * 4];

			int bitIndex = 0;
			int byteIndex = 0;

			for (int i = 0; i < inputData.Length; i++)
			{
				IQuaternionf q = inputData[i];
				byte flags = unchecked((byte)(q.X < 0 ? 4 : 0));

				float max = System.Math.Abs(q.X);
				if (System.Math.Abs(q.Y) > max)
				{
					max = System.Math.Abs(q.Y);
					flags = 1;
					if (q.Y < 0)
						flags |= 4;
				}
				if (System.Math.Abs(q.Z) > max)
				{
					max = System.Math.Abs(q.Z);
					flags = 2;
					if (q.Z < 0)
						flags |= 4;
				}
				if (System.Math.Abs(q.W) > max)
				{
					max = System.Math.Abs(q.W);
					flags = 3;
					if (q.W < 0)
						flags |= 4;
				}
				int bitOffset = 0;
				while (bitOffset < 3)
				{
					packedVector.Data[byteIndex] |= unchecked((byte)((flags >> bitOffset) << bitIndex));
					int num = System.Math.Min(3 - bitOffset, 8 - bitIndex);
					bitIndex += num;
					bitOffset += num;
					if (bitIndex == 8)
					{
						byteIndex++;
						bitIndex = 0;
					}
				}
				for (int j = 0; j < 4; j++)
				{
					if ((flags & 3) != j)
					{
						int bitSize = (((flags & 3) + 1) % 4 == j) ? 9 : 10;
						double scaled = (q.GetAt(j) + 1d) * 0.5d;
						if (scaled < 0) 
							scaled = 0d;
						else if (scaled > 1) 
							scaled = 1d;

						float f = BitConverter.Int32BitsToSingle((1 << bitSize) - 1);
						uint x = BitConverter.SingleToUInt32Bits((float)(scaled * f));

						bitOffset = 0;
						while (bitOffset < bitSize)
						{
							packedVector.Data[byteIndex] |= unchecked((byte)((x >> bitOffset) << bitIndex));
							int read = System.Math.Min(bitSize - bitOffset, 8 - bitIndex);
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
			}
		}

		public static Quaternionf[] Unpack(this IPackedQuatVector packedVector)
		{
			int bitIndex = 0;
			int byteIndex = 0;
			Quaternionf[] buffer = new Quaternionf[packedVector.NumItems];
			for (int i = 0; i < packedVector.NumItems; i++)
			{
				uint flags = 0;
				int bitOffset = 0;
				while (bitOffset < 3)
				{
					flags |= unchecked((uint)(packedVector.Data[byteIndex] >> bitIndex << bitOffset));
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

				double sum = 0;
				Quaternionf quaternion = new Quaternionf();
				for (int j = 0; j < 4; j++)
				{
					if ((flags & 3) != j)
					{
						int bitSize = (((flags & 3) + 1) % 4 == j) ? 9 : 10;

						uint value = 0;
						bitOffset = 0;
						while (bitOffset < bitSize)
						{
							value |= unchecked((uint)(packedVector.Data[byteIndex] >> bitIndex << bitOffset));
							int num = System.Math.Min(bitSize - bitOffset, 8 - bitIndex);
							bitIndex += num;
							bitOffset += num;
							if (bitIndex == 8)
							{
								byteIndex++;
								bitIndex = 0;
							}
						}
						value &= unchecked((uint)((1 << bitSize) - 1));

						// final value's range is [-1.0f : 1.0f]
						double halfMaxValue = 0.5d * ((1 << bitSize) - 1);
						double quaternion_j = value / halfMaxValue - 1.0d;
						quaternion.SetAt(j, (float)quaternion_j);
						sum += quaternion_j * quaternion_j;
					}
				}

				int lastComponent = unchecked((int)(flags & 3));
				quaternion.SetAt(lastComponent, (float)System.Math.Sqrt(1.0d - sum));
				if ((flags & 4) != 0)
					quaternion.FlipSignAt(lastComponent);
				buffer[i] = quaternion;
			}
			return buffer;
		}
	}
}
