using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct PackedFloatVector : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			NumItems = stream.ReadUInt32();
			Range = stream.ReadSingle();
			Start = stream.ReadSingle();
			m_data = stream.ReadByteArray();
			stream.AlignStream(AlignType.Align4);
			BitSize = stream.ReadByte();
			stream.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_NumItems", NumItems);
			node.Add("m_Range", Range);
			node.Add("m_Start", Start);
			node.Add("m_Data", Data == null ? YAMLSequenceNode.Empty : Data.ExportYAML());
			node.Add("m_BitSize", BitSize);
			return node;
		}

		public uint NumItems { get; private set; }
		public float Range { get; private set; }
		public float Start { get; private set; }
		public IReadOnlyList<byte> Data => m_data;
		public byte BitSize { get; private set; }

		private byte[] m_data;
	}
}
