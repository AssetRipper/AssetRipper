﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.LightmapSettings
{
	public sealed class EnlightenSystemAtlasInformation : IAsset
	{
		public void Read(AssetReader reader)
		{
			AtlasSize = reader.ReadInt32();
			AtlasHash.Read(reader);
			FirstSystemId = reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(AtlasSize);
			AtlasHash.Write(writer);
			writer.Write(FirstSystemId);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(AtlasSizeName, AtlasSize);
			node.Add(AtlasHashName, AtlasHash.ExportYaml(container));
			node.Add(FirstSystemIdName, FirstSystemId);
			return node;
		}

		public int AtlasSize { get; set; }
		public int FirstSystemId { get; set; }

		public const string AtlasSizeName = "atlasSize";
		public const string AtlasHashName = "atlasHash";
		public const string FirstSystemIdName = "firstSystemId";

		public Hash128 AtlasHash = new();
	}
}
