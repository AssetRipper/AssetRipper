using System;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Meshes
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

		public void Read(AssetStream stream)
		{
			Stream = stream.ReadByte();
			Offset = stream.ReadByte();
			Format = (ChannelFormat)stream.ReadByte();
			Dimension = stream.ReadByte();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("stream", Stream);
			node.Add("offset", Offset);
			node.Add("format", (byte)Format);
			node.Add("dimension", Dimension);
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
	}
}
