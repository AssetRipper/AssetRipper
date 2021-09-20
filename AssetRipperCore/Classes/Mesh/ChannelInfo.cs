using AssetRipper.Core.Classes.Shader.Enums.VertexFormat;
using AssetRipper.Core.Converters.Mesh;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Mesh
{
	public struct ChannelInfo : IAsset
	{
		public ChannelInfo(byte stream, byte offset, byte format, byte rawDimention)
		{
			Stream = stream;
			Offset = offset;
			Format = format;
			m_RawDimension = rawDimention;
		}

		public byte GetStride(UnityVersion version)
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
		private byte m_RawDimension;
		public byte RawDimension 
		{
			get => m_RawDimension;
			set => m_RawDimension = value;
		}
		/// <summary>
		/// Data dimention: Vector3, Vector2, Vector1
		/// </summary>
		public byte Dimension
		{
			get => (byte) (m_RawDimension & 0b00001111);
			set => m_RawDimension = (byte)((m_RawDimension & 0b11110000) | (value & 0b00001111));
		}
		public const string StreamName = "stream";
		public const string OffsetName = "offset";
		public const string FormatName = "format";
		public const string DimensionName = "dimension";
	}
}
