using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	public struct ChannelInfo : IAssetReadable, IYAMLExportable
	{
		public ChannelInfo(byte stream, byte offset, byte format, byte dimention)
		{
			Stream = stream;
			Offset = offset;
			Format = format;
			RawDimension = dimention;
		}

		public static byte CalculateStride(ChannelFormat format, int dimention)
		{
			return (byte)(format.GetSize() * dimention);
		}
		
		public byte GetStride(Version version)
		{
			ChannelFormat format = GetFormat(version);
			return CalculateStride(format, Dimension);
		}

		public ChannelFormat GetFormat(Version version)
		{
			if (version.IsLess(5))
			{
				return ((ChannelFormatV4)Format).ToChannelFormat();
			}
			else if (version.IsLess(2019))
			{
				return ((ChannelFormatV5)Format).ToChannelFormat();
			}
			else
			{
				return ((ChannelFormatV2019)Format).ToChannelFormat();
			}
		}

		public ChannelInfo ConvertToV5(Version version)
		{
			if (version.IsLess(5))
			{
				ChannelFormatV4 formatv4 = (ChannelFormatV4)Format;
				if (formatv4 == ChannelFormatV4.Color)
				{
					// replace ChannelFormat.Color with 1 dimention to ChannelFormat.Byte with 4 dimention
					return new ChannelInfo(Stream, Offset, IsSet ? (byte)ChannelFormatV5.Byte : (byte)0, IsSet ? (byte)4 : (byte)0);
				}
				else
				{
					ChannelFormatV5 format = GetFormat(version).ToChannelFormatV5();
					return new ChannelInfo(Stream, Offset, (byte)format, Dimension);
				}
			}
			else if (version.IsLess(2019))
			{
				return this;
			}
			else
			{
				ChannelFormatV5 format = GetFormat(version).ToChannelFormatV5();
				return new ChannelInfo(Stream, Offset, (byte)format, Dimension);
			}
		}

		public void Read(AssetReader reader)
		{
			Stream = reader.ReadByte();
			Offset = reader.ReadByte();
			Format = reader.ReadByte();
			RawDimension = reader.ReadByte();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(StreamName, Stream);
			node.Add(OffsetName, Offset);
			node.Add(FormatName, Format);
			node.Add(DimensionName, RawDimension);
			return node;
		}

		public override string ToString()
		{
			return $"S[{Stream}];\tO[{Offset}];\tF[{Format}];\tD[{RawDimension}]";
		}

		public bool IsSet => RawDimension > 0;

		public byte Stream { get; private set; }
		public byte Offset { get; private set; }
		public byte Format { get; private set; }
		public byte RawDimension { get; private set; }
		public byte Dimension => (byte)(RawDimension & 0xF);

		public const string StreamName = "stream";
		public const string OffsetName = "offset";
		public const string FormatName = "format";
		public const string DimensionName = "dimension";
	}
}
