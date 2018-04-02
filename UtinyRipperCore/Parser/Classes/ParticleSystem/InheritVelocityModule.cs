using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class InheritVelocityModule : ParticleSystemModule
	{
		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			Mode = stream.ReadInt32();
			Curve.Read(stream);
		}

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.Add("m_Mode", Mode);
			node.Add("m_Curve", Curve.ExportYAML(exporter));
			return node;
		}

		public int Mode { get; private set; }

		public MinMaxCurve Curve;
	}
}
