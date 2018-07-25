using System;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Meshes
{
	public struct ChannelInfo : IAssetReadable, IYAMLExportable
	{
		public ChannelInfo(byte stream, byte offset, byte format, byte dimention)
		{
			Stream = stream;
			Offset = offset;
			Format = format;
			Dimension = dimention;
		}

		public static byte CalculateStride(int format, int dimention)
		{
			int elementSize = (4 / (int)Math.Pow(2, format));
			return (byte)(elementSize * dimention);
		}

		public byte GetStride()
		{
			return CalculateStride(Format, Dimension);
		}

		public void Read(AssetStream stream)
		{
			Stream = stream.ReadByte();
			Offset = stream.ReadByte();
			Format = stream.ReadByte();
			Dimension = stream.ReadByte();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("stream", Stream);
			node.Add("offset", Offset);
			node.Add("format", Format);
			node.Add("dimension", Dimension);
			return node;
		}

		public byte Stream { get; private set; }
		public byte Offset { get; private set; }
		public byte Format { get; private set; }
		public byte Dimension { get; private set; }
	}
}
