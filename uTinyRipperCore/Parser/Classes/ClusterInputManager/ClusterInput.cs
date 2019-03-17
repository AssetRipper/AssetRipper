using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ClusterInputManagers
{
	public struct ClusterInput : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			DeviceName = reader.ReadString();
			ServerUrl = reader.ReadString();
			Index = reader.ReadInt32();
			Type = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Name", Name);
			node.Add("m_DeviceName", DeviceName);
			node.Add("m_ServerUrl", ServerUrl);
			node.Add("m_Index", Index);
			node.Add("m_Type", Type);
			return node;
		}

		public string Name { get; private set; }
		public string DeviceName { get; private set; }
		public string ServerUrl { get; private set; }
		public int Index { get; private set; }
		public int Type { get; private set; }
	}
}
