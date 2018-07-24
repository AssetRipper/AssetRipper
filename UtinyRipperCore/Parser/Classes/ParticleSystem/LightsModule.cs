using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.ParticleSystems
{
	public sealed class LightsModule : ParticleSystemModule, IDependent
	{
		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			Ratio = stream.ReadSingle();
			Light.Read(stream);
			RandomDistribution = stream.ReadBoolean();
			Color = stream.ReadBoolean();
			Range = stream.ReadBoolean();
			Intensity = stream.ReadBoolean();
			RangeCurve.Read(stream);
			IntensityCurve.Read(stream);
			MaxLights = stream.ReadInt32();
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
