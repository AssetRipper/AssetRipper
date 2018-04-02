using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class ParticleSystemModule : IAssetReadable, IYAMLExportable
	{
		public virtual void Read(AssetStream stream)
		{
			Enabled = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
		}

		public virtual YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("enabled", Enabled);
			return node;
		}

		public bool Enabled { get; private set; }
	}
}
