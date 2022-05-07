﻿using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.TerrainData
{
	public sealed class Shift : IAsset
	{
		public void Read(AssetReader reader)
		{
			X = reader.ReadUInt16();
			Y = reader.ReadUInt16();
			Flags = reader.ReadUInt16();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Flags);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(XName, X);
			node.Add(YName, Y);
			node.Add(FlagsName, Flags);
			return node;
		}

		public ushort X { get; set; }
		public ushort Y { get; set; }
		public ushort Flags { get; set; }

		public const string XName = "x";
		public const string YName = "y";
		public const string FlagsName = "flags";
	}
}
