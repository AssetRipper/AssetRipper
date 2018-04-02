using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct SpriteData : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Sprite.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("sprite", Sprite.ExportYAML(exporter));
			return node;
		}

		public PPtr<Object> Sprite;
	}
}
