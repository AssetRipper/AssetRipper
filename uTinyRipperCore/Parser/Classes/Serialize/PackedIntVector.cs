using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public struct PackedIntVector : IAssetReadable, IYAMLExportable
	{
		public int[] Unpack()
		{
			int bitIndex = 0;
			int byteIndex = 0;
			int[] buffer = new int[NumItems];
			for (int i = 0; i < NumItems; i++)
			{
				int bitOffset = 0;
				buffer[i] = 0;
				while (bitOffset < BitSize)
				{
					buffer[i] |= (Data[byteIndex] >> bitIndex) << bitOffset;
					int read = Math.Min(BitSize - bitOffset, 8 - bitIndex);
					bitIndex += read;
					bitOffset += read;
					if (bitIndex == 8)
					{
						byteIndex++;
						bitIndex = 0;
					}
				}
				buffer[i] &= (1 << BitSize) - 1;
			}
			return buffer;
		}

		public void Read(AssetReader reader)
		{
			NumItems = reader.ReadUInt32();
			m_data = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);
			BitSize = reader.ReadByte();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_NumItems", NumItems);
			node.Add("m_Data", Data == null ? YAMLSequenceNode.Empty : Data.ExportYAML());
			node.Add("m_BitSize", BitSize);
			return node;
		}

		public uint NumItems { get; private set; }
		public IReadOnlyList<byte> Data => m_data;
		public byte BitSize { get; private set; }

		private byte[] m_data;
	}
}
