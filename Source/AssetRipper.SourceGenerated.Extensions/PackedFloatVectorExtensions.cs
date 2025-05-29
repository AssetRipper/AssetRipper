using AssetRipper.SourceGenerated.Subclasses.PackedBitVector_Single;
using System.Runtime.InteropServices;

namespace AssetRipper.SourceGenerated.Extensions;

public static class PackedFloatVectorExtensions
{
	extension(PackedBitVector_Single packedVector)
	{
		public bool IsSet => packedVector.NumItems > 0;

		public float[] Unpack()
		{
			return packedVector.Unpack(1, 1);
		}

		public float[] Unpack(int itemCountInChunk, int chunkStride, int start = 0, int numChunks = -1)
		{
			if (chunkStride < itemCountInChunk)
			{
				throw new ArgumentException("Chunk stride must be at least as large as the item count in each chunk.");
			}

			if (numChunks == -1)
			{
				numChunks = (int)packedVector.NumItems / chunkStride;
			}

			PackedBitReader reader = new(packedVector, start);
			float[] result = new float[numChunks * itemCountInChunk];
			for (int chunk = 0; chunk < numChunks; chunk++)
			{
				for (int i = 0; i < itemCountInChunk; i++)
				{
					result[chunk * itemCountInChunk + i] = reader.Read();
				}
				for (int i = chunkStride - itemCountInChunk; i > 0; i--)
				{
					reader.Read(); // Skip to the next chunk
				}
			}
			return result;
		}

		/// <summary>
		/// A high accuracy default packing method for any <see langword="unmanaged""/> type.
		/// </summary>
		/// <param name="packedVector"></param>
		/// <param name="data"></param>
		public void Pack<T>(ReadOnlySpan<T> data) where T : unmanaged
		{
			packedVector.Pack(MemoryMarshal.Cast<T, float>(data));
		}

		/// <summary>
		/// A high accuracy default packing method
		/// </summary>
		/// <param name="packedVector"></param>
		/// <param name="data"></param>
		public void Pack(ReadOnlySpan<float> data)
		{
			packedVector.Pack(data, 24, false);
		}

		public void Pack(ReadOnlySpan<float> data, int bitSize, bool adjustBitSize)
		{
			GetMinimumAndMaximum(data, out float minf, out float maxf);

			float range = maxf - minf;

			if (adjustBitSize)
			{
				bitSize += GetBitCount(range);
			}

			if (bitSize > 32)
			{
				bitSize = 32;
			}

			packedVector.Start = minf;
			packedVector.Range = range;
			packedVector.NumItems = (uint)data.Length;
			packedVector.BitSize = (byte)bitSize;
			packedVector.Data = new byte[(packedVector.NumItems * bitSize + 7) / 8];

			float f = BitConverter.UInt32BitsToSingle((1u << packedVector.BitSize) - 1u);
			double scale = 1.0d / packedVector.Range;

			int bitIndex = 0;
			int byteIndex = 0;

			for (int i = 0; i < data.Length; ++i)
			{
				double scaled = double.Clamp((data[i] - packedVector.Start) * scale, 0, 1);
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
	}

	private ref struct PackedBitReader
	{
		private readonly PackedBitVector_Single packedVector;
		private int bitIndex;
		private int byteIndex;
		private readonly float scale;
		private readonly uint mask;
		private readonly float halfMaxValue;

		public PackedBitReader(PackedBitVector_Single packedVector, int start)
		{
			this.packedVector = packedVector;
			bitIndex = packedVector.BitSize * start;
			byteIndex = bitIndex / 8;
			bitIndex %= 8;
			scale = 1.0f / packedVector.Range;
			mask = (1u << packedVector.BitSize) - 1u;
			halfMaxValue = scale * mask;
		}

		public float Read()
		{
			uint x = 0;

			int bits = 0;
			int bitSize = packedVector.BitSize;
			while (bits < bitSize)
			{
				x |= (uint)packedVector.Data[byteIndex] >> bitIndex << bits;
				int read = Math.Min(bitSize - bits, 8 - bitIndex);
				bitIndex += read;
				bits += read;
				if (bitIndex == 8)
				{
					byteIndex++;
					bitIndex = 0;
				}
			}
			x &= mask;
			return x / halfMaxValue + packedVector.Start;
		}
	}

	private static void GetMinimumAndMaximum(ReadOnlySpan<float> data, out float min, out float max)
	{
		min = float.PositiveInfinity;
		max = float.NegativeInfinity;
		for (int i = 0; i < data.Length; ++i)
		{
			if (max < data[i])
			{
				max = data[i];
			}

			if (min > data[i])
			{
				min = data[i];
			}
		}
	}

	private static int GetBitCount(double value)
	{
		double log = Math.Log2(value);
		return (int)Math.Ceiling(log);
	}
}
