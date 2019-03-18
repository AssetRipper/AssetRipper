using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Lights
{
	public struct LightmapBakeMode : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			LightmapBakeType = (LightmapBakeType)reader.ReadInt32();
			MixedLightingMode = (MixedLightingMode)reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
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
