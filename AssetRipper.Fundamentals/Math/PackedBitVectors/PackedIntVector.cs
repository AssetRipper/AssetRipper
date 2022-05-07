﻿using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;
using System;
using System.Linq;

namespace AssetRipper.Core.Math.PackedBitVectors
{
	public sealed class PackedIntVector : IPackedIntVector
	{
		public const string NumItemsName = "m_NumItems";
		public const string DataName = "m_Data";
		public const string BitSizeName = "m_BitSize";

		public uint NumItems { get; set; }
		public byte[] Data { get; set; } = Array.Empty<byte>();
		public byte BitSize { get; set; }

		public void Read(AssetReader reader)
		{
			NumItems = reader.ReadUInt32();
			Data = reader.ReadByteArray();
			reader.AlignStream();
			BitSize = reader.ReadByte();
			reader.AlignStream();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(NumItems);
			writer.Write(Data);
			writer.AlignStream();
			writer.Write(BitSize);
			writer.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer _)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(NumItemsName, NumItems);
			node.Add(DataName, Data == null ? YamlSequenceNode.Empty : Data.ExportYaml());
			node.Add(BitSizeName, BitSize);
			return node;
		}
	}
}
