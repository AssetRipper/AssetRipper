using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Lights
{
	public struct LightmapBakeMode : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			LightmapBakeType = (LightmapBakeType)stream.ReadInt32();
			MixedLightingMode = (MixedLightingMode)stream.ReadInt32();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("lightmapBakeType", (int)LightmapBakeType);
			node.Add("mixedLightingMode", (int)MixedLightingMode);
			return node;
		}

		public LightmapBakeType LightmapBakeType { get; private set; }
		public MixedLightingMode MixedLightingMode { get; private set; }
	}
}
