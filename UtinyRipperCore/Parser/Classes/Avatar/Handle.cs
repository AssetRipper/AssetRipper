using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Avatars
{
	public struct Handle : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			X.Read(stream);
			ParentHumanIndex = stream.ReadUInt32();
			ID = stream.ReadUInt32();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_X", X.ExportYAML(exporter));
			node.Add("m_ParentHumanIndex", ParentHumanIndex);
			node.Add("m_ID", ID);
			return node;
		}

		public uint ParentHumanIndex { get; private set; }
		public uint ID { get; private set; }

		public XForm X;
	}
}
