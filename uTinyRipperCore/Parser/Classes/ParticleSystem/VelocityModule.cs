using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class VelocityModule : ParticleSystemModule
	{
		/// <summary>
		/// 2018.1.1 and greater
		/// </summary>
		public static bool HasOrbital(Version version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasSpeedModifier(Version version) => version.IsGreaterEqual(2017, 3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			X.Read(reader);
			Y.Read(reader);
			Z.Read(reader);
			if (HasOrbital(reader.Version))
			{
				OrbitalX.Read(reader);
				OrbitalY.Read(reader);
				OrbitalZ.Read(reader);
				OrbitalOffsetX.Read(reader);
				OrbitalOffsetY.Read(reader);
				OrbitalOffsetZ.Read(reader);
				Radial.Read(reader);
			}
			if (HasSpeedModifier(reader.Version))
			{
				SpeedModifier.Read(reader);
			}
			InWorldSpace = reader.ReadBoolean();
			reader.AlignStream();
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add(XName, X.ExportYAML(container));
			node.Add(YName, Y.ExportYAML(container));
			node.Add(ZName, Z.ExportYAML(container));
			node.Add(SpeedModifierName, GetSpeedModifier(container.Version).ExportYAML(container));
			node.Add(InWorldSpaceName, InWorldSpace);
			return node;
		}

		private MinMaxCurve GetSpeedModifier(Version version)
		{
			return HasSpeedModifier(version) ? SpeedModifier : new MinMaxCurve(1.0f);
		}
		private MinMaxCurve GetOrbitalX(Version version)
		{
			return HasOrbital(version) ? OrbitalX : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetOrbitalY(Version version)
		{
			return HasOrbital(version) ? OrbitalY : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetOrbitalZ(Version version)
		{
			return HasOrbital(version) ? OrbitalZ : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetOrbitalOffsetX(Version version)
		{
			return HasOrbital(version) ? OrbitalOffsetX : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetOrbitalOffsetY(Version version)
		{
			return HasOrbital(version) ? OrbitalOffsetY : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetOrbitalOffsetZ(Version version)
		{
			return HasOrbital(version) ? OrbitalOffsetZ : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetRadial(Version version)
		{
			return HasOrbital(version) ? Radial : new MinMaxCurve(0.0f);
		}

		public bool InWorldSpace { get; set; }

		public const string XName = "x";
		public const string YName = "y";
		public const string ZName = "z";
		public const string SpeedModifierName = "speedModifier";
		public const string InWorldSpaceName = "inWorldSpace";

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
