using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.LightmapSettingss
{
	public struct EnlightenSystemAtlasInformation : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			AtlasSize = reader.ReadInt32();
			AtlasHash.Read(reader);
			FirstSystemId = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("atlasSize", AtlasSize);
			node.Add("atlasHash", AtlasHash.ExportYAML(container));
			node.Add("firstSystemId", FirstSystemId);
			return node;
		}

		public int AtlasSize { get; private set; }
		public int FirstSystemId { get; private set; }

		public Hash128 AtlasHash;
	}
}
