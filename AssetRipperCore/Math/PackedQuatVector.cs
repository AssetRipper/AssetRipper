using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;

namespace AssetRipper.Core.Math
{
	public struct PackedQuatVector : IAssetReadable, IYAMLExportable
	{
		public const string NumItemsName = "m_NumItems";
		public const string DataName = "m_Data";

		public uint m_NumItems;
		public byte[] m_Data;

		public Quaternionf[] Unpack()
		{
			int bitIndex = 0;
			int byteIndex = 0;
			Quaternionf[] buffer = new Quaternionf[m_NumItems];
			for (int i = 0; i < m_NumItems; i++)
			{
				int flags = 0;
				int bitOffset = 0;
				while (bitOffset < 3)
				{
					flags |= m_Data[byteIndex] >> bitIndex << bitOffset;
					int read = System.Math.Min(3 - bitOffset, 8 - bitIndex);
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
							value |= m_Data[byteIndex] >> bitIndex << bitOffset;
							int num = System.Math.Min(bitSize - bitOffset, 8 - bitIndex);
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
				quaternion[lastComponent] = (float)System.Math.Sqrt(1.0f - sum);
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
			m_NumItems = reader.ReadUInt32();
			m_Data = reader.ReadByteArray();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NumItemsName, m_NumItems);
			node.Add(DataName, m_Data == null ? YAMLSequenceNode.Empty : m_Data.ExportYAML());
			return node;
		}

	}
}
