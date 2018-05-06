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

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add("gradient", Gradient.ExportYAML(container));
			return node;
		}

		public MinMaxGradient Gradient;
	}
}
