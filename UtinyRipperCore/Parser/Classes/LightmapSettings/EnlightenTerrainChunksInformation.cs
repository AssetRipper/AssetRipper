using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.LightmapSettingss
{
	public struct EnlightenTerrainChunksInformation : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			FirstSystemId = stream.ReadInt32();
			NumChunksInX = stream.ReadInt32();
			NumChunksInY = stream.ReadInt32();
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
