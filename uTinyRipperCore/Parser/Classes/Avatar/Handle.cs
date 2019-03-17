using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Avatars
{
	public struct Handle : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			X.Read(reader);
			ParentHumanIndex = reader.ReadUInt32();
			ID = reader.ReadUInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_X", X.ExportYAML(container));
			node.Add("m_ParentHumanIndex", ParentHumanIndex);
			node.Add("m_ID", ID);
			return node;
		}

		public uint ParentHumanIndex { get; private set; }
		public uint ID { get; private set; }

		public XForm X;
	}
}
