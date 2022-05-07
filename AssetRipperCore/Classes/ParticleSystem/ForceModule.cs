using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ParticleSystem
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

		public override YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = (YamlMappingNode)base.ExportYaml(container);
			node.Add(XName, X.ExportYaml(container));
			node.Add(YName, Y.ExportYaml(container));
			node.Add(ZName, Z.ExportYaml(container));
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

		public MinMaxCurve X = new();
		public MinMaxCurve Y = new();
		public MinMaxCurve Z = new();
	}
}
