using AssetRipper.Project;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;

namespace AssetRipper.Classes.Light
{
	public class LightmapBakeMode : IAssetReadable, IYAMLExportable
	{
		public LightmapBakeMode() { }
		public void Read(AssetReader reader)
		{
			LightmapBakeType = (LightmapBakeType)reader.ReadInt32();
			MixedLightingMode = (MixedLightingMode)reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(LightmapBakeTypeName, (int)LightmapBakeType);
			node.Add(MixedLightingModeName, (int)MixedLightingMode);
			return node;
		}

		public LightmapBakeType LightmapBakeType { get; set; }
		public MixedLightingMode MixedLightingMode { get; set; }

		public const string LightmapBakeTypeName = "lightmapBakeType";
		public const string MixedLightingModeName = "mixedLightingMode";
	}
}
