using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class LightsModule : ParticleSystemModule
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

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.Add("ratio", Ratio);
			node.Add("light", Light.ExportYAML(exporter));
			node.Add("randomDistribution", RandomDistribution);
			node.Add("color", Color);
			node.Add("range", Range);
			node.Add("intensity", Intensity);
			node.Add("rangeCurve", RangeCurve.ExportYAML(exporter));
			node.Add("intensityCurve", IntensityCurve.ExportYAML(exporter));
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
