using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ParticleSystem
{
	public sealed class VelocityModule : ParticleSystemModule
	{
		/// <summary>
		/// 2018.1.1 and greater
		/// </summary>
		public static bool HasOrbital(UnityVersion version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasSpeedModifier(UnityVersion version) => version.IsGreaterEqual(2017, 3);

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

		private MinMaxCurve GetSpeedModifier(UnityVersion version)
		{
			return HasSpeedModifier(version) ? SpeedModifier : new MinMaxCurve(1.0f);
		}
		private MinMaxCurve GetOrbitalX(UnityVersion version)
		{
			return HasOrbital(version) ? OrbitalX : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetOrbitalY(UnityVersion version)
		{
			return HasOrbital(version) ? OrbitalY : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetOrbitalZ(UnityVersion version)
		{
			return HasOrbital(version) ? OrbitalZ : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetOrbitalOffsetX(UnityVersion version)
		{
			return HasOrbital(version) ? OrbitalOffsetX : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetOrbitalOffsetY(UnityVersion version)
		{
			return HasOrbital(version) ? OrbitalOffsetY : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetOrbitalOffsetZ(UnityVersion version)
		{
			return HasOrbital(version) ? OrbitalOffsetZ : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetRadial(UnityVersion version)
		{
			return HasOrbital(version) ? Radial : new MinMaxCurve(0.0f);
		}

		public bool InWorldSpace { get; set; }

		public const string XName = "x";
		public const string YName = "y";
		public const string ZName = "z";
		public const string SpeedModifierName = "speedModifier";
		public const string InWorldSpaceName = "inWorldSpace";

		public MinMaxCurve X = new();
		public MinMaxCurve Y = new();
		public MinMaxCurve Z = new();
		public MinMaxCurve OrbitalX = new();
		public MinMaxCurve OrbitalY = new();
		public MinMaxCurve OrbitalZ = new();
		public MinMaxCurve OrbitalOffsetX = new();
		public MinMaxCurve OrbitalOffsetY = new();
		public MinMaxCurve OrbitalOffsetZ = new();
		public MinMaxCurve Radial = new();
		public MinMaxCurve SpeedModifier = new();
	}
}
