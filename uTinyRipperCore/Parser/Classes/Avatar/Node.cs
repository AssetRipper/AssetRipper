using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Avatars
{
	public struct Node : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			ParentId = reader.ReadInt32();
			AxesId = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_ParentId", ParentId);
			node.Add("m_AxesId", AxesId);
			return node;
		}

		public override string ToString()
		{
			return $"P:{ParentId} A:{AxesId}";
		}

		public int ParentId { get; private set; }
		public int AxesId { get; private set; }
	}
}
