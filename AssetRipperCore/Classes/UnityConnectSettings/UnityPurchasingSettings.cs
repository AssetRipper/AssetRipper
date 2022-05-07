using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.UnityConnectSettings
{
	public sealed class UnityPurchasingSettings : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			TestMode = reader.ReadBoolean();
			reader.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
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
