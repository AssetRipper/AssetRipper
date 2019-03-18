using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.OcclusionCullingSettingses
{
	public struct OcclusionBakeSettings : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			SmallestOccluder = reader.ReadSingle();
			SmallestHole = reader.ReadSingle();
			BackfaceThreshold = reader.ReadSingle();
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
