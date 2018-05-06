using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class NoiseModule : ParticleSystemModule
	{
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool IsReadPositionAmount(Version version)
		{
			return version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		}
		
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		}

		private MinMaxCurve GetExportPositionAmount(Version version)
		{
			return IsReadPositionAmount(version) ? PositionAmount : new MinMaxCurve(1.0f);
		}
		private MinMaxCurve GetExportRotationAmount(Version version)
		{
			return IsReadPositionAmount(version) ? RotationAmount : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetExportSizeAmount(Version version)
		{
			return IsReadPositionAmount(version) ? SizeAmount : new MinMaxCurve(0.0f);
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			Strength.Read(stream);
			StrengthY.Read(stream);
			StrengthZ.Read(stream);
			SeparateAxes = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			Frequency = stream.ReadSingle();
			Damping = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			Octaves = stream.ReadInt32();
			OctaveMultiplier = stream.ReadSingle();
			OctaveScale = stream.ReadSingle();
			Quality = stream.ReadInt32();
			ScrollSpeed.Read(stream);
			Remap.Read(stream);
			RemapY.Read(stream);
			RemapZ.Read(stream);
			RemapEnabled = stream.ReadBoolean();
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			if (IsReadPositionAmount(stream.Version))
			{
				PositionAmount.Read(stream);
				RotationAmount.Read(stream);
				SizeAmount.Read(stream);
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add("strength", Strength.ExportYAML(container));
			node.Add("strengthY", StrengthY.ExportYAML(container));
			node.Add("strengthZ", StrengthZ.ExportYAML(container));
			node.Add("separateAxes", SeparateAxes);
			node.Add("frequency", Frequency);
			node.Add("damping", Damping);
			node.Add("octaves", Octaves);
			node.Add("octaveMultiplier", OctaveMultiplier);
			node.Add("octaveScale", OctaveScale);
			node.Add("quality", Quality);
			node.Add("scrollSpeed", ScrollSpeed.ExportYAML(container));
			node.Add("remap", Remap.ExportYAML(container));
			node.Add("remapY", RemapY.ExportYAML(container));
			node.Add("remapZ", RemapZ.ExportYAML(container));
			node.Add("remapEnabled", RemapEnabled);
			node.Add("positionAmount", GetExportPositionAmount(container.Version).ExportYAML(container));
			node.Add("rotationAmount", GetExportRotationAmount(container.Version).ExportYAML(container));
			node.Add("sizeAmount", GetExportSizeAmount(container.Version).ExportYAML(container));
			return node;
		}

		public bool SeparateAxes { get; private set; }
		public float Frequency { get; private set; }
		public bool Damping { get; private set; }
		public int Octaves { get; private set; }
		public float OctaveMultiplier { get; private set; }
		public float OctaveScale { get; private set; }
		public int Quality { get; private set; }
		public bool RemapEnabled { get; private set; }

		public MinMaxCurve Strength;
		public MinMaxCurve StrengthY;
		public MinMaxCurve StrengthZ;
		public MinMaxCurve ScrollSpeed;
		public MinMaxCurve Remap;
		public MinMaxCurve RemapY;
		public MinMaxCurve RemapZ;
		public MinMaxCurve PositionAmount;
		public MinMaxCurve RotationAmount;
		public MinMaxCurve SizeAmount;
	}
}
