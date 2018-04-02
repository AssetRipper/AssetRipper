using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class ColorModule : ParticleSystemModule
	{
		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Gradient.Read(stream);
		}

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.Add("gradient", Gradient.ExportYAML(exporter));
			return node;
		}

		public MinMaxGradient Gradient;
	}
}
