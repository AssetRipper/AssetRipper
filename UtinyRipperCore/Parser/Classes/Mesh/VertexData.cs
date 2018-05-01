using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

		private uint GetExportCurrentChannels(Version version)
		{
			if (IsReadChannels(version))
			{
				return CurrentChannels;
			}
			else
			{
				BitArray curBits = CurrentChannelsBits;
				curBits.Set((int)ChannelType.Tangents, curBits.Get((int)ChannelType.TangentsOld));
				curBits.Set((int)ChannelType.TangentsOld, false);
				return curBits.ToUInt32();
			}
		}

		private IEnumerable<ChannelInfo> GetExportChannels(Version version)
		{
			if (IsReadChannels(version))
			{
				return m_channels;
			}

			List<ChannelInfo> channels = new List<ChannelInfo>();
			for (byte i = 0; i < Streams.Count; i++)
			{
				StreamInfo stream = Streams[i];
				if (stream.ChannelMask == 0)
				{
					continue;
				}

				BitArray streamChannels = stream.ChannelMaskBits;
				byte offset = 0;
				for (int j = 0; j < 6; j++)
				{
					byte format = 0;
					byte dimention = 0;
					ChannelInfo channel;

					if ((ChannelType)j == ChannelType.TangentsOld)
					{
						// UV3
						channels.Add(new ChannelInfo(i, 0, 0, 0));
						// UV4
						channels.Add(new ChannelInfo(i, 0, 0, 0));
					}

					if (streamChannels.Get(j))
					{
						switch ((ChannelType)j)
						{
							case ChannelType.Vertex:
							case ChannelType.Normal:
								format = 0;
								dimention = 3;
								break;

							case ChannelType.Color:
								format = 2;
								dimention = 4;
								break;

							case ChannelType.UV1:
							case ChannelType.UV2:
								format = 0;
								dimention = 2;
								break;

							case ChannelType.TangentsOld:
								format = 0;
								dimention = 4;
								break;

							default:
								throw new Exception($"Unsupported channel type {j}");
						}
						channel = new ChannelInfo(i, offset, format, dimention);
					}
					else
					{
						channel = new ChannelInfo(i, 0, 0, 0);
					}

					channels.Add(channel);

					int elementSize = (4 / (int)Math.Pow(2, format));
					int stride = elementSize * dimention;
					offset += (byte)stride;
				}
			}
			return channels;
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

			node.Add("m_CurrentChannels", GetExportCurrentChannels(exporter.Version));
			node.Add("m_VertexCount", VertexCount);
			IEnumerable<ChannelInfo> channels = GetExportChannels(exporter.Version);
			node.Add("m_Channels", (channels == null) ? YAMLSequenceNode.Empty : channels.ExportYAML(exporter));

			node.Add("m_DataSize", (m_data == null) ? 0 : m_data.Length);
			node.Add("_typelessdata", (m_data == null) ? YAMLSequenceNode.Empty : m_data.ExportYAML());
			return node;
		}

		public BitArray CurrentChannelsBits => new BitArray(BitConverter.GetBytes(CurrentChannels));

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
