using AssetRipper.Converters.Project;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.YAML;

namespace AssetRipper.Parser.Classes.UnityConnectSettings
{
	public struct PerformanceReportingSettings : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			reader.AlignStream();

		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(EnabledName, Enabled);
			return node;
		}

		public bool Enabled { get; set; }

		public const string EnabledName = "m_Enabled";
	}
}
