using uTinyRipper.Converters;
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
			node.Add(FirstSystemIdName, FirstSystemId);
			node.Add(NumChunksInXName, NumChunksInX);
			node.Add(NumChunksInYName, NumChunksInY);
			return node;
		}

		public int FirstSystemId { get; private set; }
		public int NumChunksInX { get; private set; }
		public int NumChunksInY { get; private set; }

		public const string FirstSystemIdName = "firstSystemId";
		public const string NumChunksInXName = "numChunksInX";
		public const string NumChunksInYName = "numChunksInY";
	}
}
