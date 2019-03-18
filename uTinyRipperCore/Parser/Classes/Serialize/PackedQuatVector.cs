using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public struct PackedQuatVector : IAssetReadable, IYAMLExportable
	{
		public Quaternionf[] Unpack()
		{
			int bitIndex = 0;
			int byteIndex = 0;
			Quaternionf[] buffer = new Quaternionf[NumItems];
			for (int i = 0; i < NumItems; i++)
			{
				int flags = 0;
				int bitOffset = 0;
				while (bitOffset < 3)
				{
					flags |= (Data[byteIndex] >> bitIndex) << bitOffset;
					int read = Math.Min(3 - bitOffset, 8 - bitIndex);
					bitIndex += read;
					bitOffset += read;
					if (bitIndex == 8)
					{
						byteIndex++;
						bitIndex = 0;
					}
				}
				flags &= 7;

				float sum = 0;
				Quaternionf quaternion = new Quaternionf();
				for (int j = 0; j < 4; j++)
				{
					if ((flags & 3) != j)
					{
						int bitSize = ((flags & 3) + 1) % 4 == j ? 9 : 10;
						float halfMaxValue = 0.5f * ((1 << bitSize) - 1);

						int value = 0;
						bitOffset = 0;
						while (bitOffset < bitSize)
						{
							value |= (Data[byteIndex] >> bitIndex) << bitOffset;
							int num = Math.Min(bitSize - bitOffset, 8 - bitIndex);
							bitIndex += num;
							bitOffset += num;
							if (bitIndex == 8)
							{
								byteIndex++;
								bitIndex = 0;
							}
						}
						value &= (1 << bitSize) - 1;
						// final value's range is [-1.0f : 1.0f]
						quaternion[j] = value / halfMaxValue - 1.0f;
						sum += quaternion[j] * quaternion[j];
					}
				}

				int lastComponent = flags & 3;
				quaternion[lastComponent] = (float)Math.Sqrt(1.0f - sum);
				if ((flags & 4) != 0)
				{
					quaternion[lastComponent] = -quaternion[lastComponent];
				}
				buffer[i] = quaternion;
			}
			return buffer;
		}

		public void Read(AssetReader reader)
		{
			NumItems = reader.ReadUInt32();
			m_data = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_NumItems", NumItems);
			node.Add("m_Data", Data == null ? YAMLSequenceNode.Empty : Data.ExportYAML());
			return node;
		}

		public uint NumItems { get; private set; }
		public IReadOnlyList<byte> Data => m_data;

		private byte[] m_data;
	}
}
