using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
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
			node.Add(RendererIndexName, RendererIndex);
			node.Add(RendererSizeName, RendererSize);
			node.Add(AtlasIndexName, AtlasIndex);
			node.Add(AtlasOffsetXName, AtlasOffsetX);
			node.Add(AtlasOffsetYName, AtlasOffsetY);
			node.Add(InputSystemHashName, InputSystemHash.ExportYAML(container));
			node.Add(RadiositySystemHashName, RadiositySystemHash.ExportYAML(container));
			return node;
		}

		public uint RendererIndex { get; private set; }
		public uint RendererSize { get; private set; }
		public int AtlasIndex { get; private set; }
		public int AtlasOffsetX { get; private set; }
		public int AtlasOffsetY { get; private set; }

		public const string RendererIndexName = "rendererIndex";
		public const string RendererSizeName = "rendererSize";
		public const string AtlasIndexName = "atlasIndex";
		public const string AtlasOffsetXName = "atlasOffsetX";
		public const string AtlasOffsetYName = "atlasOffsetY";
		public const string InputSystemHashName = "inputSystemHash";
		public const string RadiositySystemHashName = "radiositySystemHash";

		public Hash128 InputSystemHash;
		public Hash128 RadiositySystemHash;
	}
}
