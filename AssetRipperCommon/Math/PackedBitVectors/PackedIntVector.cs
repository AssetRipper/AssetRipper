using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;
using System.Linq;

namespace AssetRipper.Core.Math.PackedBitVectors
{
	public sealed class PackedIntVector : IAsset, IPackedIntVector
	{
		public const string NumItemsName = "m_NumItems";
		public const string DataName = "m_Data";
		public const string BitSizeName = "m_BitSize";

		public uint NumItems { get; set; }
		public byte[] Data { get; set; } = Array.Empty<byte>();
		public byte BitSize { get; set; }

		public static PackedIntVector Pack(int[] values)
		{
			throw new NotImplementedException();
		}

		public PackedIntVector Convert(IExportContainer _)
		{
			PackedIntVector instance = new();
			instance.NumItems = NumItems;
			instance.Data = Data.ToArray();
			instance.BitSize = BitSize;
			return instance;
		}

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

		public YAMLNode ExportYAML(IExportContainer _)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NumItemsName, NumItems);
			node.Add(DataName, Data == null ? YAMLSequenceNode.Empty : Data.ExportYAML());
			node.Add(BitSizeName, BitSize);
			return node;
		}
	}
}
