using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct PackedIntVector : IAssetReadable, IYAMLExportable
	{
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
