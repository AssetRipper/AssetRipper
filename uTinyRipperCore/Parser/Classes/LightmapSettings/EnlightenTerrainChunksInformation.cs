using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.LightmapSettingss
{
	public struct EnlightenTerrainChunksInformation : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			FirstSystemId = reader.ReadInt32();
			NumChunksInX = reader.ReadInt32();
			NumChunksInY = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("firstSystemId", FirstSystemId);
			node.Add("numChunksInX", NumChunksInX);
			node.Add("numChunksInY", NumChunksInY);
			return node;
		}

		public int FirstSystemId { get; private set; }
		public int NumChunksInX { get; private set; }
		public int NumChunksInY { get; private set; }
	}
}
