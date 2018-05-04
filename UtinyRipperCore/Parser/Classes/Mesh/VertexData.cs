using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Meshes
{
	public struct VertexData : IAssetReadable, IYAMLExportable
	{
		public VertexData(Version version, IReadOnlyList<Vector3f> vertices, IReadOnlyList<Vector3f> normals, IReadOnlyList<ColorRGBA32> colors,
			IReadOnlyList<Vector2f> uv0, IReadOnlyList<Vector2f> uv1, IReadOnlyList<Vector4f> tangents)
		{
			BitArray curChannels = new BitArray(8);
			byte stride = 0;

			bool isWriteVertices = vertices.Count > 0;
			bool isWriteNormals = normals != null && normals.Count > 0;
			bool isWriteColors = colors.Count > 0;
			bool isWriteUV0 = uv0.Count > 0;
			bool isWriteUV1 = uv1 != null && uv1.Count > 0;
			bool isWriteTangents = tangents != null && tangents.Count > 0;

			if (isWriteVertices)
			{
				curChannels.Set((int)ChannelType.Vertex, true);
				stride += ChannelType.Vertex.GetStride(version);
			}
			if (isWriteNormals)
			{
				curChannels.Set((int)ChannelType.Normal, true);
				stride += ChannelType.Normal.GetStride(version);
			}
			if(isWriteColors)
			{
				curChannels.Set((int)ChannelType.Color, true);
				stride += ChannelType.Color.GetStride(version);
			}
			if (isWriteUV0)
			{
				curChannels.Set((int)ChannelType.UV0, true);
				stride += ChannelType.UV0.GetStride(version);
			}
			if (isWriteUV1)
			{
				curChannels.Set((int)ChannelType.UV1, true);
				stride += ChannelType.UV1.GetStride(version);
			}
			if (isWriteTangents)
			{
				curChannels.Set((int)ChannelType.TangentsOld, true);
				stride += ChannelType.TangentsOld.GetStride(version);
			}

			CurrentChannels = curChannels.ToUInt32();
			VertexCount = unchecked((uint)vertices.Count);
			m_channels = null;

			StreamInfo info = new StreamInfo(CurrentChannels, 0, stride);
			m_streams = new StreamInfo[] { info, default, default, default, };

			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					for (int i = 0; i < VertexCount; i++)
					{
						vertices[i].Write(writer);
						if (isWriteNormals)
						{
							normals[i].Write(writer);
						}
						if(isWriteColors)
						{
							colors[i].Write(writer);
						}
						if (isWriteColors)
						{
							colors[i].Write(writer);
						}
						if (isWriteUV0)
						{
							uv0[i].Write(writer);
						}
						if (isWriteUV1)
						{
							uv1[i].Write(writer);
						}
						if (isWriteTangents)
						{
							tangents[i].Write(writer);
						}
					}
				}
				m_data = stream.ToArray();
			}
		}

		/// <summary>
		/// Less than 2018.1
		/// </summary>
		public static bool IsReadCurrentChannels(Version version)
		{
			return version.IsLess(2018);
		}
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
			if(IsReadCurrentChannels(stream.Version))
			{
				CurrentChannels = stream.ReadUInt32();
			}
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

		private uint GetExportCurrentChannels(Version version)
		{
			if(IsReadCurrentChannels(version))
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
			else
			{
#warning not sure it is right approach
				BitArray curChannels = new BitArray(32);
				int prevStream = 0;
				for(int i = 0, j = 0; i < Channels.Count; i++, j++)
				{
					ChannelInfo channel = Channels[i];
					if(channel.Stream != prevStream)
					{
						prevStream = channel.Stream;
						j = 0;
					}

					curChannels.Set(j, channel.Dimension != 0);
				}
				return curChannels.ToUInt32();
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
						ChannelType channelType = (ChannelType)j;
						channel = new ChannelInfo(i, offset, channelType.GetFormat(version), channelType.GetDimention(version));
						offset += channelType.GetStride(version);
					}
					else
					{
						channel = new ChannelInfo(i, 0, 0, 0);
					}

					channels.Add(channel);
				}
			}
			return channels;
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
