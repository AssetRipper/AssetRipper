using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class ExternalForcesModule : ParticleSystemModule
	{
		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			Multiplier = stream.ReadSingle();
		}

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.Add("multiplier", Multiplier);
			return node;
		}

		public float Multiplier { get; private set; }
	}
}
