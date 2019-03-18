using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class ColorBySpeedModule : ParticleSystemModule
	{
		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			Gradient.Read(reader);
			Range.Read(reader);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add("gradient", Gradient.ExportYAML(container));
			node.Add("range", Range.ExportYAML(container));
			return node;
		}

		public MinMaxGradient Gradient;
		public Vector2f Range;
	}
}
