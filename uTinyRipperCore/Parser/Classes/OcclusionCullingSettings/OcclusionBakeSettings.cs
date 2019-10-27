using uTinyRipper.Converters;
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
			node.Add(SmallestOccluderName, SmallestOccluder);
			node.Add(SmallestHoleName, SmallestHole);
			node.Add(BackfaceThresholdName, BackfaceThreshold);
			return node;
		}

		public float SmallestOccluder { get; set; }
		public float SmallestHole { get; set; }
		public float BackfaceThreshold { get; set; }

		public const string SmallestOccluderName = "smallestOccluder";
		public const string SmallestHoleName = "smallestHole";
		public const string BackfaceThresholdName = "backfaceThreshold";
	}
}
