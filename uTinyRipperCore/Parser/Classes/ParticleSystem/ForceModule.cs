using uTinyRipper.Converters;
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
			reader.AlignStream();
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add(XName, X.ExportYAML(container));
			node.Add(YName, Y.ExportYAML(container));
			node.Add(ZName, Z.ExportYAML(container));
			node.Add(InWorldSpaceName, InWorldSpace);
			node.Add(RandomizePerFrameName, RandomizePerFrame);
			return node;
		}

		public bool InWorldSpace { get; set; }
		public bool RandomizePerFrame { get; set; }

		public const string XName = "x";
		public const string YName = "y";
		public const string ZName = "z";
		public const string InWorldSpaceName = "inWorldSpace";
		public const string RandomizePerFrameName = "randomizePerFrame";

		public MinMaxCurve X;
		public MinMaxCurve Y;
		public MinMaxCurve Z;
	}
}
