using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.LightmapSettingss
{
	public struct EnlightenTerrainChunksInformation : IAsset
	{
		public void Read(AssetReader reader)
		{
			FirstSystemId = reader.ReadInt32();
			NumChunksInX = reader.ReadInt32();
			NumChunksInY = reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(FirstSystemId);
			writer.Write(NumChunksInX);
			writer.Write(NumChunksInY);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(FirstSystemIdName, FirstSystemId);
			node.Add(NumChunksInXName, NumChunksInX);
			node.Add(NumChunksInYName, NumChunksInY);
			return node;
		}

		public int FirstSystemId { get; set; }
		public int NumChunksInX { get; set; }
		public int NumChunksInY { get; set; }

		public const string FirstSystemIdName = "firstSystemId";
		public const string NumChunksInXName = "numChunksInX";
		public const string NumChunksInYName = "numChunksInY";
	}
}
