using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;
using System.Linq;

namespace AssetRipper.Core.Math.PackedBitVectors
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

		public static PackedIntVector Pack(int[] values)
		{
			throw new NotImplementedException();
		}

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

		public PackedIntVector Convert(IExportContainer _)
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

		public YAMLNode ExportYAML(IExportContainer _)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NumItemsName, m_NumItems);
			node.Add(DataName, m_Data == null ? YAMLSequenceNode.Empty : m_Data.ExportYAML());
			node.Add(BitSizeName, m_BitSize);
			return node;
		}
	}
}
