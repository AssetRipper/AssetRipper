using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.UnityConnectSettingss
{
	public struct UnityPurchasingSettings : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			TestMode = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Enabled", Enabled);
			node.Add("m_TestMode", TestMode);
			return node;
		}

		public bool Enabled { get; private set; }
		public bool TestMode { get; private set; }
	}
}
