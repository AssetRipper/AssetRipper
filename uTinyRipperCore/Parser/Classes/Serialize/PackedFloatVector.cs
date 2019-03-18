using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public struct PackedFloatVector : IAssetReadable, IYAMLExportable
	{
		public float[] Unpack()
		{
			return Unpack(NumItems, 0);
		}

		public float[] Unpack(int chunkCount, int offset)
		{
			int bitIndex = (BitSize * offset) % 8;
			int byteIndex = (BitSize * offset) / 8;

			float scale = 1.0f / Range;
			float halfMaxValue = scale * ((1 << BitSize) - 1);
			float[] buffer = new float[chunkCount];

			for (int i = 0; i < chunkCount; i++)
			{
				int value = 0;
				int bits = 0;
				while (bits < BitSize)
				{
					value |= (Data[byteIndex] >> bitIndex) << bits;
					int num = Math.Min(BitSize - bits, 8 - bitIndex);
					bitIndex += num;
					bits += num;
					if (bitIndex == 8)
					{
						byteIndex++;
						bitIndex = 0;
					}
				}
				value &= (1 << BitSize) - 1;
				buffer[i] = Start + value / halfMaxValue;
			}
			return buffer;
		}

		public float[] Unpack(int chunkSize, int chunkCount, int offset)
		{
			return Unpack(chunkSize * chunkCount, offset);
		}

		public void Read(AssetReader reader)
		{
			NumItems = (int)reader.ReadUInt32();
			Range = reader.ReadSingle();
			Start = reader.ReadSingle();
			m_data = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);
			BitSize = reader.ReadByte();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_NumItems", NumItems);
			node.Add("m_Range", Range);
			node.Add("m_Start", Start);
			node.Add("m_Data", Data == null ? YAMLSequenceNode.Empty : Data.ExportYAML());
			node.Add("m_BitSize", BitSize);
			return node;
		}

		public int NumItems { get; private set; }
		public float Range { get; private set; }
		public float Start { get; private set; }
		public IReadOnlyList<byte> Data => m_data;
		public byte BitSize { get; private set; }

		private byte[] m_data;
	}
}
