using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.Math.PackedBitVectors
{
	public struct PackedFloatVector : IAsset
	{
		public const string NumItemsName = "m_NumItems";
		public const string RangeName = "m_Range";
		public const string StartName = "m_Start";
		public const string DataName = "m_Data";
		public const string BitSizeName = "m_BitSize";

		public uint m_NumItems;
		public float m_Range;
		public float m_Start;
		public byte[] m_Data;
		public byte m_BitSize;

		public bool IsSet => m_NumItems > 0;

		public PackedFloatVector(bool _) : this()
		{
			m_Data = Array.Empty<byte>();
		}

		public static PackedFloatVector Pack(float[] values)
		{
			throw new NotImplementedException();
		}

		public float[] Unpack()
		{
			return Unpack(m_NumItems, 0);
		}

		public float[] Unpack(uint chunkCount, int offset)
		{
			int bitIndex = m_BitSize * offset % 8;
			int byteIndex = m_BitSize * offset / 8;

			float scale = 1.0f / m_Range;
			float halfMaxValue = scale * ((1 << m_BitSize) - 1);
			float[] buffer = new float[chunkCount];

			for (int i = 0; i < chunkCount; i++)
			{
				int value = 0;
				int bits = 0;
				while (bits < m_BitSize)
				{
					value |= m_Data[byteIndex] >> bitIndex << bits;
					int num = System.Math.Min(m_BitSize - bits, 8 - bitIndex);
					bitIndex += num;
					bits += num;
					if (bitIndex == 8)
					{
						byteIndex++;
						bitIndex = 0;
					}
				}
				value &= (1 << m_BitSize) - 1;
				buffer[i] = m_Start + value / halfMaxValue;
			}
			return buffer;
		}

		public float[] Unpack(uint chunkSize, uint chunkCount, int offset)
		{
			return Unpack(chunkSize * chunkCount, offset);
		}

		public float[] UnpackFloats(int itemCountInChunk, int chunkStride, int start = 0, int numChunks = -1)
		{
			int bitPos = m_BitSize * start;
			int indexPos = bitPos / 8;
			bitPos %= 8;

			float scale = 1.0f / m_Range;
			if (numChunks == -1)
				numChunks = (int)m_NumItems / itemCountInChunk;
			var end = chunkStride * numChunks / 4;
			var data = new List<float>();
			for (var index = 0; index != end; index += chunkStride / 4)
				for (int i = 0; i < itemCountInChunk; ++i)
				{
					uint x = 0;

					int bits = 0;
					while (bits < m_BitSize)
					{
						x |= (uint)(m_Data[indexPos] >> bitPos << bits);
						int num = System.Math.Min(m_BitSize - bits, 8 - bitPos);
						bitPos += num;
						bits += num;
						if (bitPos == 8)
						{
							indexPos++;
							bitPos = 0;
						}
					}
					x &= (uint)(1 << m_BitSize) - 1u;
					data.Add(x / (scale * ((1 << m_BitSize) - 1)) + m_Start);
				}

			return data.ToArray();
		}

		public PackedFloatVector Convert(IExportContainer _)
		{
			PackedFloatVector instance = this;
			instance.m_Data = m_Data.ToArray();
			return instance;
		}

		public void Read(AssetReader reader)
		{
			m_NumItems = reader.ReadUInt32();
			m_Range = reader.ReadSingle();
			m_Start = reader.ReadSingle();
			m_Data = reader.ReadByteArray();
			reader.AlignStream();
			m_BitSize = reader.ReadByte();
			reader.AlignStream();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(m_NumItems);
			writer.Write(m_Range);
			writer.Write(m_Start);
			writer.Write(m_Data);
			writer.AlignStream();
			writer.Write(m_BitSize);
			writer.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer _)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NumItemsName, m_NumItems);
			node.Add(RangeName, m_Range);
			node.Add(StartName, m_Start);
			node.Add(DataName, m_Data.ExportYAML());
			node.Add(BitSizeName, m_BitSize);
			return node;
		}
	}
}
