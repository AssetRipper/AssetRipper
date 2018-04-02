using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class ForceModule : ParticleSystemModule
	{
		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			X.Read(stream);
			Y.Read(stream);
			Z.Read(stream);
			InWorldSpace = stream.ReadBoolean();
			RandomizePerFrame = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
		}

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.Add("x", X.ExportYAML(exporter));
			node.Add("y", Y.ExportYAML(exporter));
			node.Add("z", Z.ExportYAML(exporter));
			node.Add("inWorldSpace", InWorldSpace);
			node.Add("randomizePerFrame", RandomizePerFrame);
			return node;
		}

		public bool InWorldSpace { get; private set; }
		public bool RandomizePerFrame { get; private set; }

		public MinMaxCurve X;
		public MinMaxCurve Y;
		public MinMaxCurve Z;
	}
}
