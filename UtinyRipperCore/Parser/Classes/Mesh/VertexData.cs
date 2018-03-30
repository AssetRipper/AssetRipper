using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Meshes
{
	public struct VertexData : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadChannels(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadStream(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// 3.5.0
		/// </summary>
		public static bool IsStreamSpecial(Version version)
		{
			return version.IsEqual(3, 5);
		}
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		private static bool IsStreamStatic(Version version)
		{
			return version.IsLess(4);
		}

		public void Read(AssetStream stream)
		{
			CurrentChannels = stream.ReadUInt32();
			VertexCount = stream.ReadUInt32();

			if (IsReadChannels(stream.Version))
			{
				m_channels = stream.ReadArray<ChannelInfo>();
				stream.AlignStream(AlignType.Align4);
			}
			if (IsReadStream(stream.Version))
			{
				if (IsStreamStatic(stream.Version))
				{
					m_streams = new StreamInfo[4];
					for (int i = 0; i < 4; i++)
					{
						StreamInfo streamInfo = new StreamInfo();
						streamInfo.Read(stream);
						m_streams[i] = streamInfo;
					}
				}
				else
				{
					m_streams = stream.ReadArray<StreamInfo>();
				}
			}

			m_data = stream.ReadByteArray();
			stream.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_CurrentChannels", CurrentChannels);
			node.Add("m_VertexCount", VertexCount);
			node.Add("m_Channels", (m_channels == null) ? YAMLSequenceNode.Empty : m_channels.ExportYAML(exporter));
			node.Add("m_DataSize", (m_data == null) ? 0 : m_data.Length);
			node.Add("_typelessdata", (m_data == null) ? YAMLSequenceNode.Empty : m_data.ExportYAML());
			return node;
		}

		public uint CurrentChannels { get; private set; }
		public uint VertexCount { get; private set; }
		public IReadOnlyList<ChannelInfo> Channels => m_channels;
		public IReadOnlyList<StreamInfo> Streams => m_streams;
		public IReadOnlyList<byte> Data => m_data;

		private ChannelInfo[] m_channels;
		private StreamInfo[] m_streams;
		private byte[] m_data;
	}
}
