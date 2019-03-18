using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class VelocityModule : ParticleSystemModule
	{
		/// <summary>
		/// 2018.1.1 and greater
		/// </summary>
		public static bool IsReadOrbital(Version version)
		{
			return version.IsGreaterEqual(2018);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadSpeedModifier(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			X.Read(reader);
			Y.Read(reader);
			Z.Read(reader);
			if (IsReadOrbital(reader.Version))
			{
				OrbitalX.Read(reader);
				OrbitalY.Read(reader);
				OrbitalZ.Read(reader);
				OrbitalOffsetX.Read(reader);
				OrbitalOffsetY.Read(reader);
				OrbitalOffsetZ.Read(reader);
				Radial.Read(reader);
			}
			if (IsReadSpeedModifier(reader.Version))
			{
				SpeedModifier.Read(reader);
			}
			InWorldSpace = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
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
		private MinMaxCurve GetOrbitalX(Version version)
		{
			return IsReadOrbital(version) ? OrbitalX : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetOrbitalY(Version version)
		{
			return IsReadOrbital(version) ? OrbitalY : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetOrbitalZ(Version version)
		{
			return IsReadOrbital(version) ? OrbitalZ : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetOrbitalOffsetX(Version version)
		{
			return IsReadOrbital(version) ? OrbitalOffsetX : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetOrbitalOffsetY(Version version)
		{
			return IsReadOrbital(version) ? OrbitalOffsetY : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetOrbitalOffsetZ(Version version)
		{
			return IsReadOrbital(version) ? OrbitalOffsetZ : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetRadial(Version version)
		{
			return IsReadOrbital(version) ? Radial : new MinMaxCurve(0.0f);
		}

		public bool InWorldSpace { get; private set; }

		public MinMaxCurve X;
		public MinMaxCurve Y;
		public MinMaxCurve Z;
		public MinMaxCurve OrbitalX;
		public MinMaxCurve OrbitalY;
		public MinMaxCurve OrbitalZ;
		public MinMaxCurve OrbitalOffsetX;
		public MinMaxCurve OrbitalOffsetY;
		public MinMaxCurve OrbitalOffsetZ;
		public MinMaxCurve Radial;
		public MinMaxCurve SpeedModifier;
	}
}
