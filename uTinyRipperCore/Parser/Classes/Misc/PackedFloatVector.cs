using System;
using System.Linq;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes;

namespace uTinyRipper.Classes.Misc
{
	public struct PackedFloatVector : IAsset
	{
		public PackedFloatVector(bool _) :
			this()
		{
			Data = Array.Empty<byte>();
		}

#warning TODO: Pack method

		public float[] Unpack()
		{
			return Unpack(NumItems, 0);
		}

		public float[] Unpack(int chunkCount, int offset)
		{
			int bitIndex = BitSize * offset % 8;
			int byteIndex = BitSize * offset / 8;

			float scale = 1.0f / Range;
			float halfMaxValue = scale * ((1 << BitSize) - 1);
			float[] buffer = new float[chunkCount];

			for (int i = 0; i < chunkCount; i++)
			{
				int value = 0;
				int bits = 0;
				while (bits < BitSize)
				{
					value |= Data[byteIndex] >> bitIndex << bits;
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

		public PackedFloatVector Convert(IExportContainer container)
		{
			PackedFloatVector instance = this;
			instance.Data = Data.ToArray();
			return instance;
		}

		public void Read(AssetReader reader)
		{
			NumItems = (int)reader.ReadUInt32();
			Range = reader.ReadSingle();
			Start = reader.ReadSingle();
			Data = reader.ReadByteArray();
			reader.AlignStream();
			BitSize = reader.ReadByte();
			reader.AlignStream();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(NumItems);
			writer.Write(Range);
			writer.Write(Start);
			writer.Write(Data);
			writer.AlignStream();
			writer.Write(BitSize);
			writer.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NumItemsName, NumItems);
			node.Add(RangeName, Range);
			node.Add(StartName, Start);
			node.Add(DataName, Data.ExportYAML());
			node.Add(BitSizeName, BitSize);
			return node;
		}

		public bool IsSet => NumItems > 0;

		public int NumItems { get; set; }
		public float Range { get; set; }
		public float Start { get; set; }
		public byte[] Data { get; set; }
		public byte BitSize { get; set; }

		public const string NumItemsName = "m_NumItems";
		public const string RangeName = "m_Range";
		public const string StartName = "m_Start";
		public const string DataName = "m_Data";
		public const string BitSizeName = "m_BitSize";
	}
}
