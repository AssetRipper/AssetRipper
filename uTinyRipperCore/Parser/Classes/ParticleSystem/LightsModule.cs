using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

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

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Light, LightName);
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

		public float Ratio { get; set; }
		public bool RandomDistribution { get; set; }
		public bool Color { get; set; }
		public bool Range { get; set; }
		public bool Intensity { get; set; }
		public int MaxLights { get; set; }

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
