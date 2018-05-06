using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.OcclusionCullingSettingses
{
	public struct OcclusionBakeSettings : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			SmallestOccluder = stream.ReadSingle();
			SmallestHole = stream.ReadSingle();
			BackfaceThreshold = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("smallestOccluder", SmallestOccluder);
			node.Add("smallestHole", SmallestHole);
			node.Add("backfaceThreshold", BackfaceThreshold);
			return node;
		}

		public float SmallestOccluder { get; set; }
		public float SmallestHole { get; set; }
		public float BackfaceThreshold { get; set; }
	}
}
