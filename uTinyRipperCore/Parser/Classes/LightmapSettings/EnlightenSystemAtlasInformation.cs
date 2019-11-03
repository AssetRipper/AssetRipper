using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.LightmapSettingss
{
	public struct EnlightenSystemAtlasInformation : IAsset
	{
		public void Read(AssetReader reader)
		{
			AtlasSize = reader.ReadInt32();
			AtlasHash.Read(reader);
			FirstSystemId = reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(AtlasSize);
			AtlasHash.Write(writer);
			writer.Write(FirstSystemId);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(AtlasSizeName, AtlasSize);
			node.Add(AtlasHashName, AtlasHash.ExportYAML(container));
			node.Add(FirstSystemIdName, FirstSystemId);
			return node;
		}

		public int AtlasSize { get; set; }
		public int FirstSystemId { get; set; }

		public const string AtlasSizeName = "atlasSize";
		public const string AtlasHashName = "atlasHash";
		public const string FirstSystemIdName = "firstSystemId";

		public Hash128 AtlasHash;
	}
}
