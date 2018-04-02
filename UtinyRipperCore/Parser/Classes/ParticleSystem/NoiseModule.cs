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

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.Add("strength", Strength.ExportYAML(exporter));
			node.Add("strengthY", StrengthY.ExportYAML(exporter));
			node.Add("strengthZ", StrengthZ.ExportYAML(exporter));
			node.Add("separateAxes", SeparateAxes);
			node.Add("frequency", Frequency);
			node.Add("damping", Damping);
			node.Add("octaves", Octaves);
			node.Add("octaveMultiplier", OctaveMultiplier);
			node.Add("octaveScale", OctaveScale);
			node.Add("quality", Quality);
			node.Add("scrollSpeed", ScrollSpeed.ExportYAML(exporter));
			node.Add("remap", Remap.ExportYAML(exporter));
			node.Add("remapY", RemapY.ExportYAML(exporter));
			node.Add("remapZ", RemapZ.ExportYAML(exporter));
			node.Add("remapEnabled", RemapEnabled);
			node.Add("positionAmount", PositionAmount.ExportYAML(exporter));
			node.Add("rotationAmount", RotationAmount.ExportYAML(exporter));
			node.Add("sizeAmount", SizeAmount.ExportYAML(exporter));
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
