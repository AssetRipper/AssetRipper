using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.UnityConnectSettingss
{
	public struct PerformanceReportingSettings : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
			
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Enabled", Enabled);
			return node;
		}

		public bool Enabled { get; private set; }
	}
}
