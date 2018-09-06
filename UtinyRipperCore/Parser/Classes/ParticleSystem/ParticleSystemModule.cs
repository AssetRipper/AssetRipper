using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class ParticleSystemModule : IAssetReadable, IYAMLExportable
	{
		public virtual void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
		}

		public virtual YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("enabled", Enabled);
			return node;
		}

		public bool Enabled { get; private set; }
	}
}
