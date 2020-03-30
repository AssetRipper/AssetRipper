using uTinyRipper.Classes.Shaders;
using uTinyRipper.Converters;
using uTinyRipper.Converters.Meshes;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	public struct ChannelInfo : IAsset
	{
		public ChannelInfo(byte stream, byte offset, byte format, byte rawDimention)
		{
			Stream = stream;
			Offset = offset;
			Format = format;
			RawDimension = rawDimention;
		}

		public byte GetStride(Version version)
		{
			return GetVertexFormat(version).CalculateStride(version, Dimension);
		}

		public ChannelInfo Convert(IExportContainer container)
		{
			return ChannelInfoConverter.Convert(container, this);
		}

		public void Read(AssetReader reader)
		{
			Stream = reader.ReadByte();
			Offset = reader.ReadByte();
			Format = reader.ReadByte();
			RawDimension = reader.ReadByte();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Stream);
			writer.Write(Offset);
			writer.Write(Format);
			writer.Write(RawDimension);
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

		public VertexFormat GetVertexFormat(Version version)
		{
			if (VertexFormatExtensions.VertexFormat2019Relevant(version))
			{
				return ((VertexFormat2019)Format).ToVertexFormat();
			}
			else if (VertexFormatExtensions.VertexFormat2017Relevant(version))
			{
				return ((VertexFormat2017)Format).ToVertexFormat();
			}
			else
			{
				return ((VertexChannelFormat)Format).ToVertexFormat();
			}
		}

		public override string ToString()
		{
			return $"S[{Stream}];\tO[{Offset}];\tF[{Format}];\tD[{RawDimension}]";
		}

		public bool IsSet => RawDimension > 0;

		/// <summary>
		/// Stream index
		/// BinaryData:[Stream0][Align][Stream1][Align]...
		/// </summary>
		public byte Stream { get; set; }
		/// <summary>
		/// Offset inside stream
		/// Stream:[FirstVertex: VertexOffset,NormalOffset,TangentOffset...][SecondVertex: VertexOffset,NormalOffset,TangentOffset...]...
		/// </summary>
		public byte Offset { get; set; }
		/// <summary>
		/// Data format: float, int, byte
		/// </summary>
		public byte Format { get; set; }
		public byte RawDimension { get; set; }
		/// <summary>
		/// Data dimention: Vector3, Vector2, Vector1
		/// </summary>
		public byte Dimension => (byte)(RawDimension & 0xF);

		public const string StreamName = "stream";
		public const string OffsetName = "offset";
		public const string FormatName = "format";
		public const string DimensionName = "dimension";
	}
}
