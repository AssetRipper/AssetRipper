using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.UnityConnectSettingss
{
	public struct UnityAnalyticsSettings : IAssetReadable, IYAMLExportable
	{
		public UnityAnalyticsSettings(bool _):
			this()
		{
			InitializeOnStartup = true;
			TestEventUrl = string.Empty;
			TestConfigUrl = string.Empty;
		}

		public void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			InitializeOnStartup = reader.ReadBoolean();
			TestMode = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
			
			TestEventUrl = reader.ReadString();
			TestConfigUrl = reader.ReadString();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Enabled", Enabled);
			node.Add("m_InitializeOnStartup", InitializeOnStartup);
			node.Add("m_TestMode", TestMode);
			node.Add("m_TestEventUrl", TestEventUrl);
			node.Add("m_TestConfigUrl", TestConfigUrl);
			return node;
		}

		public bool Enabled { get; private set; }
		public bool InitializeOnStartup { get; private set; }
		public bool TestMode { get; private set; }
		public string TestEventUrl { get; private set; }
		public string TestConfigUrl { get; private set; }
	}
}
