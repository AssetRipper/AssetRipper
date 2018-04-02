using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class ColorBySpeedModule : ParticleSystemModule
	{
		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			Gradient.Read(stream);
			Range.Read(stream);
		}

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.Add("gradient", Gradient.ExportYAML(exporter));
			node.Add("range", Range.ExportYAML(exporter));
			return node;
		}

		public MinMaxGradient Gradient;
		public Vector2f Range;
	}
}
