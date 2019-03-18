using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.LightmapSettingss
{
	public struct EnlightenSystemInformation : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			RendererIndex = reader.ReadUInt32();
			RendererSize = reader.ReadUInt32();
			AtlasIndex = reader.ReadInt32();
			AtlasOffsetX = reader.ReadInt32();
			AtlasOffsetY = reader.ReadInt32();
			InputSystemHash.Read(reader);
			RadiositySystemHash.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("rendererIndex", RendererIndex);
			node.Add("rendererSize", RendererSize);
			node.Add("atlasIndex", AtlasIndex);
			node.Add("atlasOffsetX", AtlasOffsetX);
			node.Add("atlasOffsetY", AtlasOffsetY);
			node.Add("inputSystemHash", InputSystemHash.ExportYAML(container));
			node.Add("radiositySystemHash", RadiositySystemHash.ExportYAML(container));
			return node;
		}

		public uint RendererIndex { get; private set; }
		public uint RendererSize { get; private set; }
		public int AtlasIndex { get; private set; }
		public int AtlasOffsetX { get; private set; }
		public int AtlasOffsetY { get; private set; }

		public Hash128 InputSystemHash;
		public Hash128 RadiositySystemHash;
	}
}
