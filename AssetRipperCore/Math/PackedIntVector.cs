using AssetRipper.Project;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;
using AssetRipper.YAML.Extensions;
using System;
using System.Linq;
using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Math
{
	public struct PackedIntVector : IAsset
	{
		public const string NumItemsName = "m_NumItems";
		public const string DataName = "m_Data";
		public const string BitSizeName = "m_BitSize";

		public uint m_NumItems;
		public byte[] m_Data;
		public byte m_BitSize;

		public bool IsSet => m_NumItems > 0;

		public PackedIntVector(bool _) : this()
		{
			m_Data = Array.Empty<byte>();
		}
		
		public PackedIntVector(ObjectReader reader)
		{
			m_NumItems = reader.ReadUInt32();

			int numData = reader.ReadInt32();
			m_Data = reader.ReadBytes(numData);
			reader.AlignStream();

			m_BitSize = reader.ReadByte();
			reader.AlignStream();
		}
		
#warning TODO: Pack method

		public int[] Unpack()
		{
			int bitIndex = 0;
			int byteIndex = 0;
			int[] buffer = new int[m_NumItems];
			for (int i = 0; i < m_NumItems; i++)
			{
				int bitOffset = 0;
				buffer[i] = 0;
				while (bitOffset < m_BitSize)
				{
					buffer[i] |= m_Data[byteIndex] >> bitIndex << bitOffset;
					int read = System.Math.Min(m_BitSize - bitOffset, 8 - bitIndex);
					bitIndex += read;
					bitOffset += read;
					if (bitIndex == 8)
					{
						byteIndex++;
						bitIndex = 0;
					}
				}
				buffer[i] &= (1 << m_BitSize) - 1;
			}
			return buffer;
		}

		public PackedIntVector Convert(IExportContainer container)
		{
			PackedIntVector instance = this;
			instance.m_Data = m_Data.ToArray();
			return instance;
		}

		public void Read(AssetReader reader)
		{
			m_NumItems = reader.ReadUInt32();
			m_Data = reader.ReadByteArray();
			reader.AlignStream();
			m_BitSize = reader.ReadByte();
			reader.AlignStream();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(m_NumItems);
			writer.Write(m_Data);
			writer.AlignStream();
			writer.Write(m_BitSize);
			writer.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NumItemsName, m_NumItems);
			node.Add(DataName, m_Data == null ? YAMLSequenceNode.Empty : m_Data.ExportYAML());
			node.Add(BitSizeName, m_BitSize);
			return node;
		}
	}
}
