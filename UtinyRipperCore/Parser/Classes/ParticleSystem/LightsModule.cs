using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.ParticleSystems
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
			node.Add("ratio", Ratio);
			node.Add("light", Light.ExportYAML(container));
			node.Add("randomDistribution", RandomDistribution);
			node.Add("color", Color);
			node.Add("range", Range);
			node.Add("intensity", Intensity);
			node.Add("rangeCurve", RangeCurve.ExportYAML(container));
			node.Add("intensityCurve", IntensityCurve.ExportYAML(container));
			node.Add("maxLights", MaxLights);
			return node;
		}

		public float Ratio { get; private set; }
		public bool RandomDistribution { get; private set; }
		public bool Color { get; private set; }
		public bool Range { get; private set; }
		public bool Intensity { get; private set; }
		public int MaxLights { get; private set; }

		public PPtr<Light> Light;
		public MinMaxCurve RangeCurve;
		public MinMaxCurve IntensityCurve;
	}
}
