﻿using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Light
{
	public sealed class LightmapBakeMode : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			LightmapBakeType = (LightmapBakeType)reader.ReadInt32();
			MixedLightingMode = (MixedLightingMode)reader.ReadInt32();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
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
