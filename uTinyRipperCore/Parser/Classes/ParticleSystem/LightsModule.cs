using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class LightsModule : ParticleSystemModule, IDependent
	{
		public LightsModule()
		{
		}

		public LightsModule(bool _)
		{
			RandomDistribution = true;
			Color = true;
			Range = true;
			Intensity = true;
			RangeCurve = new MinMaxCurve(1.0f);
			IntensityCurve = new MinMaxCurve(1.0f);
			MaxLights = 20;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			Ratio = reader.ReadSingle();
			Light.Read(reader);
			RandomDistribution = reader.ReadBoolean();
			Color = reader.ReadBoolean();
			Range = reader.ReadBoolean();
			Intensity = reader.ReadBoolean();
			RangeCurve.Read(reader);
			IntensityCurve.Read(reader);
			MaxLights = reader.ReadInt32();
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Light.FetchDependency(file, isLog, () => nameof(LightsModule), "light");
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add(RatioName, Ratio);
			node.Add(LightName, Light.ExportYAML(container));
			node.Add(RandomDistributionName, RandomDistribution);
			node.Add(ColorName, Color);
			node.Add(RangeName, Range);
			node.Add(IntensityName, Intensity);
			node.Add(RangeCurveName, RangeCurve.ExportYAML(container));
			node.Add(IntensityCurveName, IntensityCurve.ExportYAML(container));
			node.Add(MaxLightsName, MaxLights);
			return node;
		}

		public float Ratio { get; private set; }
		public bool RandomDistribution { get; private set; }
		public bool Color { get; private set; }
		public bool Range { get; private set; }
		public bool Intensity { get; private set; }
		public int MaxLights { get; private set; }

		public const string RatioName = "ratio";
		public const string LightName = "light";
		public const string RandomDistributionName = "randomDistribution";
		public const string ColorName = "color";
		public const string RangeName = "range";
		public const string IntensityName = "intensity";
		public const string RangeCurveName = "rangeCurve";
		public const string IntensityCurveName = "intensityCurve";
		public const string MaxLightsName = "maxLights";

		public PPtr<Light> Light;
		public MinMaxCurve RangeCurve;
		public MinMaxCurve IntensityCurve;
	}
}
