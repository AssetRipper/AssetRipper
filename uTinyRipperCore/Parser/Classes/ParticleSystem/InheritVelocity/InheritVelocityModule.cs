using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class InheritVelocityModule : ParticleSystemModule
	{
		public InheritVelocityModule()
		{
		}

		public InheritVelocityModule(float value):
			base(value != 0.0f)
		{
			Curve = new MinMaxCurve(value);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			Mode = (InheritVelocityMode)reader.ReadInt32();
			Curve.Read(reader);
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
