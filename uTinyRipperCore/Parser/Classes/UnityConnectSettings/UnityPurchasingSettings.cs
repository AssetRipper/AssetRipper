using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.UnityConnectSettingss
{
	public struct UnityPurchasingSettings : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			TestMode = reader.ReadBoolean();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(EnabledName, Enabled);
			node.Add(TestModeName, TestMode);
			return node;
		}

		public bool Enabled { get; set; }
		public bool TestMode { get; set; }

		public const string EnabledName = "m_Enabled";
		public const string TestModeName = "m_TestMode";
	}
}
