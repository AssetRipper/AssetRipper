using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public sealed class InheritVelocityModule : ParticleSystemModule
	{
		public InheritVelocityModule()
		{
		}

		public InheritVelocityModule(bool _)
		{
			Curve = new MinMaxCurve(0.0f);
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			Mode = (InheritVelocityMode)stream.ReadInt32();
			Curve.Read(stream);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add("m_Mode", (int)Mode);
			node.Add("m_Curve", Curve.ExportYAML(container));
			return node;
		}

		public InheritVelocityMode Mode { get; private set; }

		public MinMaxCurve Curve;
	}
}
