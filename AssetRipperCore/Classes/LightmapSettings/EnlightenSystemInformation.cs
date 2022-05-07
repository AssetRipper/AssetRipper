using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.LightmapSettings
{
	public sealed class EnlightenSystemInformation : IAsset
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

		public void Write(AssetWriter writer)
		{
			writer.Write(RendererIndex);
			writer.Write(RendererSize);
			writer.Write(AtlasIndex);
			writer.Write(AtlasOffsetX);
			writer.Write(AtlasOffsetY);
			InputSystemHash.Write(writer);
			RadiositySystemHash.Write(writer);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(RendererIndexName, RendererIndex);
			node.Add(RendererSizeName, RendererSize);
			node.Add(AtlasIndexName, AtlasIndex);
			node.Add(AtlasOffsetXName, AtlasOffsetX);
			node.Add(AtlasOffsetYName, AtlasOffsetY);
			node.Add(InputSystemHashName, InputSystemHash.ExportYaml(container));
			node.Add(RadiositySystemHashName, RadiositySystemHash.ExportYaml(container));
			return node;
		}

		public uint RendererIndex { get; set; }
		public uint RendererSize { get; set; }
		public int AtlasIndex { get; set; }
		public int AtlasOffsetX { get; set; }
		public int AtlasOffsetY { get; set; }

		public const string RendererIndexName = "rendererIndex";
		public const string RendererSizeName = "rendererSize";
		public const string AtlasIndexName = "atlasIndex";
		public const string AtlasOffsetXName = "atlasOffsetX";
		public const string AtlasOffsetYName = "atlasOffsetY";
		public const string InputSystemHashName = "inputSystemHash";
		public const string RadiositySystemHashName = "radiositySystemHash";

		public Hash128 InputSystemHash = new();
		public Hash128 RadiositySystemHash = new();
	}
}
