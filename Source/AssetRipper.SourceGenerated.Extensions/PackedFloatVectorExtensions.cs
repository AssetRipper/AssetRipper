using AssetRipper.SourceGenerated.Subclasses.PackedBitVector_Single;
using System.Runtime.InteropServices;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class PackedFloatVectorExtensions
	{
		public static bool IsSet(this PackedBitVector_Single packedVector) => packedVector.NumItems > 0;

		public static void CopyValuesFrom(this PackedBitVector_Single instance, PackedBitVector_Single source)
		{
			instance.NumItems = source.NumItems;
			instance.Range = source.Range;
			instance.Start = source.Start;
			instance.Data = source.Data.ToArray();
			instance.BitSize = source.BitSize;
		}

		public static float[] Unpack(this PackedBitVector_Single packedVector)
		{
			return packedVector.Unpack(packedVector.NumItems, 0);
		}

		public static float[] Unpack(this PackedBitVector_Single packedVector, uint chunkCount, int offset)
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
					int num = Math.Min(packedVector.BitSize - bits, 8 - bitIndex);
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

		public static float[] Unpack(this PackedBitVector_Single packedVector, uint chunkSize, uint chunkCount, int offset)
		{
			return packedVector.Unpack(chunkSize * chunkCount, offset);
		}

		public static float[] UnpackFloats(this PackedBitVector_Single packedVector, int itemCountInChunk, int chunkStride, int start = 0, int numChunks = -1)
		{
			if (chunkStride % 4 != 0)
			{
				throw new ArgumentException(null, nameof(chunkStride));
			}

			int bitIndex = packedVector.BitSize * start;
			int byteIndex = bitIndex / 8;
			bitIndex %= 8;

			float scale = 1.0f / packedVector.Range;
			if (numChunks == -1)
			{
				numChunks = (int)packedVector.NumItems / itemCountInChunk;
			}

			int end = chunkStride * numChunks / 4;
			List<float> data = new List<float>();
			for (int index = 0; index != end; index += chunkStride / 4)
			{
				for (int i = 0; i < itemCountInChunk; ++i)
				{
					uint x = 0;

					int bits = 0;
					while (bits < packedVector.BitSize)
					{
						x |= unchecked((uint)(packedVector.Data[byteIndex] >> bitIndex << bits));
						int read = Math.Min(packedVector.BitSize - bits, 8 - bitIndex);
						bitIndex += read;
						bits += read;
						if (bitIndex == 8)
						{
							byteIndex++;
							bitIndex = 0;
						}
					}
					x &= unchecked((uint)(1 << packedVector.BitSize) - 1u);
					data.Add(x / (scale * ((1 << packedVector.BitSize) - 1)) + packedVector.Start);
				}
			}
			return data.ToArray();
		}

		/// <summary>
		/// A high accuracy default packing method for any <see langword="unmanaged""/> type.
		/// </summary>
		/// <param name="packedVector"></param>
		/// <param name="data"></param>
		public static void Pack<T>(this PackedBitVector_Single packedVector, ReadOnlySpan<T> data) where T : unmanaged
		{
			packedVector.PackFloats(MemoryMarshal.Cast<T, float>(data));
		}

		/// <summary>
		/// A high accuracy default packing method
		/// </summary>
		/// <param name="packedVector"></param>
		/// <param name="data"></param>
		public static void PackFloats(this PackedBitVector_Single packedVector, ReadOnlySpan<float> data)
		{
			packedVector.PackFloats(data, 24, false);
		}

		public static void PackFloats(this PackedBitVector_Single packedVector, ReadOnlySpan<float> data, int itemCountInChunk, int chunkStride, int numChunks, int bitSize, bool adjustBitSize)
		{
			if (data.Length != itemCountInChunk * numChunks)
			{
				throw new ArgumentException(null, nameof(data));
			}

			if (chunkStride != itemCountInChunk * 4)
			{
				throw new ArgumentException(null, nameof(chunkStride));
			}

			packedVector.PackFloats(data, bitSize, adjustBitSize);
		}

		public static void PackFloats(this PackedBitVector_Single packedVector, ReadOnlySpan<float> data, int bitSize, bool adjustBitSize)
		{
			float maxf = float.NegativeInfinity;
			float minf = float.PositiveInfinity;
			for (int i = 0; i < data.Length; ++i)
			{
				if (maxf < data[i])
				{
					maxf = data[i];
				}

				if (minf > data[i])
				{
					minf = data[i];
				}
			}

			packedVector.Range = maxf - minf;

			if (adjustBitSize)
			{
				bitSize += GetBitCount(packedVector.Range);
			}

			if (bitSize > 32)
			{
				bitSize = 32;
			}

			packedVector.Start = minf;
			packedVector.NumItems = (uint)data.Length;
			packedVector.BitSize = (byte)bitSize;
			packedVector.Data = new byte[(packedVector.NumItems * bitSize + 7) / 8];


			double scale = 1.0d / packedVector.Range;

			int bitIndex = 0;
			int byteIndex = 0;

			for (int i = 0; i < data.Length; ++i)
			{
				double scaled = (data[i] - packedVector.Start) * scale;
				if (scaled < 0)
				{
					scaled = 0d;
				}
				else if (scaled > 1)
				{
					scaled = 1d;
				}

				float f = BitConverter.Int32BitsToSingle((1 << packedVector.BitSize) - 1);
				double d = scaled * f;
				uint x = BitConverter.SingleToUInt32Bits((float)d);

				int bits = 0;
				while (bits < packedVector.BitSize)
				{
					packedVector.Data[byteIndex] |= unchecked((byte)(x >> bits << bitIndex));
					int read = Math.Min(packedVector.BitSize - bits, 8 - bitIndex);
					bitIndex += read;
					bits += read;
					if (bitIndex == 8)
					{
						byteIndex++;
						bitIndex = 0;
					}
				}
			}
		}

		private static int GetBitCount(double value)
		{
			double log = Math.Log2(value);
			return (int)Math.Ceiling(log);
		}
	}
}
