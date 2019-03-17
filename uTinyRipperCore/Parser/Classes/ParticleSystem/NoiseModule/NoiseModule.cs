using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class NoiseModule : ParticleSystemModule
	{
		public NoiseModule()
		{
		}

		public NoiseModule(bool _)
		{
			Strength = new MinMaxCurve(1.0f);
			StrengthY = new MinMaxCurve(1.0f);
			StrengthZ = new MinMaxCurve(1.0f);
			Frequency = 0.5f;
			Damping = true;
			Octaves = 1;
			OctaveMultiplier = 0.5f;
			OctaveScale = 2.0f;
			Quality = ParticleSystemNoiseQuality.High;
			ScrollSpeed = new MinMaxCurve(0.0f);
			Remap = new MinMaxCurve(1.0f, 1.0f, 1.0f, 0.0f, 1.0f);
			RemapY = new MinMaxCurve(1.0f, 1.0f, 1.0f, 0.0f, 1.0f);
			RemapZ = new MinMaxCurve(1.0f, 1.0f, 1.0f, 0.0f, 1.0f);
			PositionAmount = new MinMaxCurve(1.0f);
			RotationAmount = new MinMaxCurve(0.0f);
			SizeAmount = new MinMaxCurve(0.0f);
		}

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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			Strength.Read(reader);
			StrengthY.Read(reader);
			StrengthZ.Read(reader);
			SeparateAxes = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
			
			Frequency = reader.ReadSingle();
			Damping = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
			
			Octaves = reader.ReadInt32();
			OctaveMultiplier = reader.ReadSingle();
			OctaveScale = reader.ReadSingle();
			Quality = (ParticleSystemNoiseQuality)reader.ReadInt32();
			ScrollSpeed.Read(reader);
			Remap.Read(reader);
			RemapY.Read(reader);
			RemapZ.Read(reader);
			RemapEnabled = reader.ReadBoolean();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadPositionAmount(reader.Version))
			{
				PositionAmount.Read(reader);
				RotationAmount.Read(reader);
				SizeAmount.Read(reader);
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
			node.Add("quality", (int)Quality);
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

		public bool SeparateAxes { get; private set; }
		public float Frequency { get; private set; }
		public bool Damping { get; private set; }
		public int Octaves { get; private set; }
		public float OctaveMultiplier { get; private set; }
		public float OctaveScale { get; private set; }
		public ParticleSystemNoiseQuality Quality { get; private set; }
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
