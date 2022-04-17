using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.UnityConnectSettings
{
	public sealed class PerformanceReportingSettings : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			reader.AlignStream();

		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(EnabledName, Enabled);
			return node;
		}

		public bool Enabled { get; set; }

		public const string EnabledName = "m_Enabled";
	}
}
