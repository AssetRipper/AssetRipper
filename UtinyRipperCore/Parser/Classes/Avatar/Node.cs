using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Avatars
{
	public struct Node : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			ParentId = stream.ReadInt32();
			AxesId = stream.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_ParentId", ParentId);
			node.Add("m_AxesId", AxesId);
			return node;
		}

		public int ParentId { get; private set; }
		public int AxesId { get; private set; }
	}
}
