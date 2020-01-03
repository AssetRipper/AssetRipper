using System;
using System.Linq;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes;

namespace uTinyRipper.Classes.Misc
{
	public struct PackedIntVector : IAsset
	{
		public PackedIntVector(bool _) :
			this()
		{
			Data = Array.Empty<byte>();
		}

#warning TODO: Pack method

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
					buffer[i] |= Data[byteIndex] >> bitIndex << bitOffset;
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

		public PackedIntVector Convert(IExportContainer container)
		{
			PackedIntVector instance = this;
			instance.Data = Data.ToArray();
			return instance;
		}

		public void Read(AssetReader reader)
		{
			NumItems = reader.ReadUInt32();
			Data = reader.ReadByteArray();
			reader.AlignStream();
			BitSize = reader.ReadByte();
			reader.AlignStream();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(NumItems);
			writer.Write(Data);
			writer.AlignStream();
			writer.Write(BitSize);
			writer.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NumItemsName, NumItems);
			node.Add(DataName, Data == null ? YAMLSequenceNode.Empty : Data.ExportYAML());
			node.Add(BitSizeName, BitSize);
			return node;
		}

		public bool IsSet => NumItems > 0;

		public uint NumItems { get; set; }
		public byte[] Data { get; set; }
		public byte BitSize { get; set; }

		public const string NumItemsName = "m_NumItems";
		public const string DataName = "m_Data";
		public const string BitSizeName = "m_BitSize";
	}
}
