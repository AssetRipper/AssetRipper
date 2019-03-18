using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class ForceModule : ParticleSystemModule
	{
		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			X.Read(reader);
			Y.Read(reader);
			Z.Read(reader);
			InWorldSpace = reader.ReadBoolean();
			RandomizePerFrame = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add("x", X.ExportYAML(container));
			node.Add("y", Y.ExportYAML(container));
			node.Add("z", Z.ExportYAML(container));
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
