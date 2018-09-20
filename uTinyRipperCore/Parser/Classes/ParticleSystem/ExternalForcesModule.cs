using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class ExternalForcesModule : ParticleSystemModule
	{
		public ExternalForcesModule()
		{
		}

		public ExternalForcesModule(bool _)
		{
			Multiplier = 1.0f;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			Multiplier = reader.ReadSingle();
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add("multiplier", Multiplier);
			return node;
		}

		public float Multiplier { get; private set; }
	}
}
