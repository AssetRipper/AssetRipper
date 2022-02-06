using AssetRipper.Core.Classes.Shader.Enums.VertexFormat;
using AssetRipper.Core.Converters.Mesh;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Mesh
{
	public sealed class ChannelInfo : IChannelInfo
	{
		public ChannelInfo() { }
		public ChannelInfo(byte stream, byte offset, byte format, byte rawDimention)
		{
			Stream = stream;
			Offset = offset;
			Format = format;
			Dimension = rawDimention;
		}

		public byte GetStride(UnityVersion version)
		{
			return GetVertexFormat(version).CalculateStride(version, this.GetDataDimension());
		}

		public VertexFormat GetVertexFormat(UnityVersion version)
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

		public ChannelInfo Convert(IExportContainer container)
		{
			return ChannelInfoConverter.Convert(container, this);
		}

		public ChannelInfo Clone()
		{
			ChannelInfo instance = new();
			instance.Stream = Stream;
			instance.Offset = Offset;
			instance.Format = Format;
			instance.Dimension = Dimension;
			return instance;
		}

		public void Read(AssetReader reader)
		{
			Stream = reader.ReadByte();
			Offset = reader.ReadByte();
			Format = reader.ReadByte();
			Dimension = reader.ReadByte();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Stream);
			writer.Write(Offset);
			writer.Write(Format);
			writer.Write(Dimension);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(StreamName, Stream);
			node.Add(OffsetName, Offset);
			node.Add(FormatName, Format);
			node.Add(DimensionName, Dimension);
			return node;
		}

		public override string ToString()
		{
			return $"S[{Stream}];\tO[{Offset}];\tF[{Format}];\tD[{Dimension}]";
		}

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
		/// <summary>
		/// An unprocessed byte value containing the data dimension
		/// </summary>
		public byte Dimension { get; set; }
		
		public const string StreamName = "stream";
		public const string OffsetName = "offset";
		public const string FormatName = "format";
		public const string DimensionName = "dimension";
	}
}
