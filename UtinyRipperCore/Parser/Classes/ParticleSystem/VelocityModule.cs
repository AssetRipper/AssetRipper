using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class VelocityModule : ParticleSystemModule
	{
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadSpeedModifier(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			X.Read(stream);
			Y.Read(stream);
			Z.Read(stream);
			if (IsReadSpeedModifier(stream.Version))
			{
				SpeedModifier.Read(stream);
			}
			InWorldSpace = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
		}

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.Add("x", X.ExportYAML(exporter));
			node.Add("y", Y.ExportYAML(exporter));
			node.Add("z", Z.ExportYAML(exporter));
			node.Add("speedModifier", SpeedModifier.ExportYAML(exporter));
			node.Add("inWorldSpace", InWorldSpace);
			return node;
		}

		public bool InWorldSpace { get; private set; }

		public MinMaxCurve X;
		public MinMaxCurve Y;
		public MinMaxCurve Z;
		public MinMaxCurve SpeedModifier;
	}
}
