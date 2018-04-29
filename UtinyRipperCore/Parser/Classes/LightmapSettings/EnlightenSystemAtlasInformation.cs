using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.LightmapSettingss
{
	public struct EnlightenSystemAtlasInformation : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			AtlasSize = stream.ReadInt32();
			AtlasHash.Read(stream);
			FirstSystemId = stream.ReadInt32();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("atlasSize", AtlasSize);
			node.Add("atlasHash", AtlasHash.ExportYAML(exporter));
			node.Add("firstSystemId", FirstSystemId);
			return node;
		}

		public int AtlasSize { get; private set; }
		public int FirstSystemId { get; private set; }

		public Hash128 AtlasHash;
	}
}
