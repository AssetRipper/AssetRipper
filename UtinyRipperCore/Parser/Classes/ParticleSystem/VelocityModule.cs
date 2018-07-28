using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public sealed class VelocityModule : ParticleSystemModule
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

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add("x", X.ExportYAML(container));
			node.Add("y", Y.ExportYAML(container));
			node.Add("z", Z.ExportYAML(container));
			node.Add("speedModifier", GetSpeedModifier(container.Version).ExportYAML(container));
			node.Add("inWorldSpace", InWorldSpace);
			return node;
		}

		private MinMaxCurve GetSpeedModifier(Version version)
		{
			return IsReadSpeedModifier(version) ? SpeedModifier : new MinMaxCurve(1.0f);
		}
		
		public bool InWorldSpace { get; private set; }

		public MinMaxCurve X;
		public MinMaxCurve Y;
		public MinMaxCurve Z;
		public MinMaxCurve SpeedModifier;
	}
}
