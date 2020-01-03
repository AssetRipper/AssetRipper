using uTinyRipper.Converters;
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
			node.Add(ParentIdName, ParentId);
			node.Add(AxesIdName, AxesId);
			return node;
		}

		public override string ToString()
		{
			return $"P:{ParentId} A:{AxesId}";
		}

		public int ParentId { get; set; }
		public int AxesId { get; set; }

		public const string ParentIdName = "m_ParentId";
		public const string AxesIdName = "m_AxesId";
	}
}
