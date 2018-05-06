using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.LightmapSettingss
{
	public struct EnlightenSystemInformation : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			RendererIndex = stream.ReadUInt32();
			RendererSize = stream.ReadUInt32();
			AtlasIndex = stream.ReadInt32();
			AtlasOffsetX = stream.ReadInt32();
			AtlasOffsetY = stream.ReadInt32();
			InputSystemHash.Read(stream);
			RadiositySystemHash.Read(stream);
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
