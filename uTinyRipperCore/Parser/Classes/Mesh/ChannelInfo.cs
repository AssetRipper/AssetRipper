using System;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	public struct ChannelInfo : IAssetReadable, IYAMLExportable
	{
		public ChannelInfo(byte stream, byte offset, ChannelFormat format, byte dimention)
		{
			Stream = stream;
			Offset = offset;
			Format = format;
			Dimension = dimention;
		}

		public static byte CalculateStride(ChannelFormat format, int dimention)
		{
			return (byte)(format.GetSize() * dimention);
		}
		
		public byte GetStride()
		{
			return CalculateStride(Format, Dimension);
		}

		public void Read(AssetReader reader)
		{
			Stream = reader.ReadByte();
			Offset = reader.ReadByte();
			Format = (ChannelFormat)reader.ReadByte();
			Dimension = reader.ReadByte();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(StreamName, Stream);
			node.Add(OffsetName, Offset);
			node.Add(FormatName, (byte)Format);
			node.Add(DimensionName, Dimension);
			return node;
		}

		public override string ToString()
		{
			return $"S[{Stream}];\tO[{Offset}];\tF[{Format}];\tD[{Dimension}]";
		}

		public bool IsSet => Dimension > 0;

		public byte Stream { get; private set; }
		public byte Offset { get; private set; }
		public ChannelFormat Format { get; private set; }
		public byte Dimension { get; private set; }

		public const string StreamName = "stream";
		public const string OffsetName = "offset";
		public const string FormatName = "format";
		public const string DimensionName = "dimension";
	}
}
