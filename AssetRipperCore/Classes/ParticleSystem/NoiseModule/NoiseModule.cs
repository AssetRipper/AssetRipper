﻿using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ParticleSystem.NoiseModule
{
	public sealed class NoiseModule : ParticleSystemModule
	{
		public NoiseModule() { }

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
		public static bool HasPositionAmount(UnityVersion version) => version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 2);

		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Strength.Read(reader);
			StrengthY.Read(reader);
			StrengthZ.Read(reader);
			SeparateAxes = reader.ReadBoolean();
			reader.AlignStream();

			Frequency = reader.ReadSingle();
			Damping = reader.ReadBoolean();
			reader.AlignStream();

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
				reader.AlignStream();
			}

			if (HasPositionAmount(reader.Version))
			{
				PositionAmount.Read(reader);
				RotationAmount.Read(reader);
				SizeAmount.Read(reader);
			}
		}

		public override YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = (YamlMappingNode)base.ExportYaml(container);
			node.Add(StrengthName, Strength.ExportYaml(container));
			node.Add(StrengthYName, StrengthY.ExportYaml(container));
			node.Add(StrengthZName, StrengthZ.ExportYaml(container));
			node.Add(SeparateAxesName, SeparateAxes);
			node.Add(FrequencyName, Frequency);
			node.Add(DampingName, Damping);
			node.Add(OctavesName, Octaves);
			node.Add(OctaveMultiplierName, OctaveMultiplier);
			node.Add(OctaveScaleName, OctaveScale);
			node.Add(QualityName, (int)Quality);
			node.Add(ScrollSpeedName, ScrollSpeed.ExportYaml(container));
			node.Add(RemapName, Remap.ExportYaml(container));
			node.Add(RemapYName, RemapY.ExportYaml(container));
			node.Add(RemapZName, RemapZ.ExportYaml(container));
			node.Add(RemapEnabledName, RemapEnabled);
			node.Add(PositionAmountName, GetExportPositionAmount(container.Version).ExportYaml(container));
			node.Add(RotationAmountName, GetExportRotationAmount(container.Version).ExportYaml(container));
			node.Add(SizeAmountName, GetExportSizeAmount(container.Version).ExportYaml(container));
			return node;
		}

		private MinMaxCurve GetExportPositionAmount(UnityVersion version)
		{
			return HasPositionAmount(version) ? PositionAmount : new MinMaxCurve(1.0f);
		}
		private MinMaxCurve GetExportRotationAmount(UnityVersion version)
		{
			return HasPositionAmount(version) ? RotationAmount : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetExportSizeAmount(UnityVersion version)
		{
			return HasPositionAmount(version) ? SizeAmount : new MinMaxCurve(0.0f);
		}

		public bool SeparateAxes { get; set; }
		public float Frequency { get; set; }
		public bool Damping { get; set; }
		public int Octaves { get; set; }
		public float OctaveMultiplier { get; set; }
		public float OctaveScale { get; set; }
		public ParticleSystemNoiseQuality Quality { get; set; }
		public bool RemapEnabled { get; set; }

		public const string StrengthName = "strength";
		public const string StrengthYName = "strengthY";
		public const string StrengthZName = "strengthZ";
		public const string SeparateAxesName = "separateAxes";
		public const string FrequencyName = "frequency";
		public const string DampingName = "damping";
		public const string OctavesName = "octaves";
		public const string OctaveMultiplierName = "octaveMultiplier";
		public const string OctaveScaleName = "octaveScale";
		public const string QualityName = "quality";
		public const string ScrollSpeedName = "scrollSpeed";
		public const string RemapName = "remap";
		public const string RemapYName = "remapY";
		public const string RemapZName = "remapZ";
		public const string RemapEnabledName = "remapEnabled";
		public const string PositionAmountName = "positionAmount";
		public const string RotationAmountName = "rotationAmount";
		public const string SizeAmountName = "sizeAmount";

		public MinMaxCurve Strength = new();
		public MinMaxCurve StrengthY = new();
		public MinMaxCurve StrengthZ = new();
		public MinMaxCurve ScrollSpeed = new();
		public MinMaxCurve Remap = new();
		public MinMaxCurve RemapY = new();
		public MinMaxCurve RemapZ = new();
		public MinMaxCurve PositionAmount = new();
		public MinMaxCurve RotationAmount = new();
		public MinMaxCurve SizeAmount = new();
	}
}
