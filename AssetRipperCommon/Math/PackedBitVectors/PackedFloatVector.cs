using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;

namespace AssetRipper.Core.Math.PackedBitVectors
{
	public sealed class PackedFloatVector : IPackedFloatVector
	{
		public const string NumItemsName = "m_NumItems";
		public const string RangeName = "m_Range";
		public const string StartName = "m_Start";
		public const string DataName = "m_Data";
		public const string BitSizeName = "m_BitSize";

		public uint NumItems { get; set; }
		public float Range { get; set; }
		public float Start { get; set; }
		public byte[] Data { get; set; } = Array.Empty<byte>();
		public byte BitSize { get; set; }

		public void Read(AssetReader reader)
		{
			NumItems = reader.ReadUInt32();
			Range = reader.ReadSingle();
			Start = reader.ReadSingle();
			Data = reader.ReadByteArray();
			reader.AlignStream();
			BitSize = reader.ReadByte();
			reader.AlignStream();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(NumItems);
			writer.Write(Range);
			writer.Write(Start);
			writer.Write(Data);
			writer.AlignStream();
			writer.Write(BitSize);
			writer.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer _)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NumItemsName, NumItems);
			node.Add(RangeName, Range);
			node.Add(StartName, Start);
			node.Add(DataName, Data.ExportYAML());
			node.Add(BitSizeName, BitSize);
			return node;
		}
	}
}
