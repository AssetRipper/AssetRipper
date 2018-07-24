using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public sealed class InheritVelocityModule : ParticleSystemModule
	{
		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			Mode = stream.ReadInt32();
			Curve.Read(stream);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add("m_Mode", Mode);
			node.Add("m_Curve", Curve.ExportYAML(container));
			return node;
		}

		public int Mode { get; private set; }

		public MinMaxCurve Curve;
	}
}
