﻿using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class ComputeBufferCounter : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Bindpoint = reader.ReadInt32();
			Offset = reader.ReadInt32();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("bindpoint", Bindpoint);
			node.Add("offset", Offset);
			return node;
		}

		public int Bindpoint { get; set; }

		public int Offset { get; set; }
	}
}
