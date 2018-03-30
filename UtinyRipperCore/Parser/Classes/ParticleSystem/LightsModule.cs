using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct LightsModule : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetStream stream)
		{
			Enabled = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
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

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("enabled", Enabled);
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

		public bool Enabled { get; private set; }
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
