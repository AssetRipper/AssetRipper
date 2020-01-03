using uTinyRipper.Converters;
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
			node.Add(NameName, Name);
			node.Add(DeviceNameName, DeviceName);
			node.Add(ServerUrlName, ServerUrl);
			node.Add(IndexName, Index);
			node.Add(TypeName, Type);
			return node;
		}

		public string Name { get; set; }
		public string DeviceName { get; set; }
		public string ServerUrl { get; set; }
		public int Index { get; set; }
		public int Type { get; set; }

		public const string NameName = "m_Name";
		public const string DeviceNameName = "m_DeviceName";
		public const string ServerUrlName = "m_ServerUrl";
		public const string IndexName = "m_Index";
		public const string TypeName = "m_Type";
	}
}
