using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Meshes
{
	public struct VertexData : IAssetReadable, IYAMLExportable
	{
		public VertexData(Version version, IReadOnlyList<Vector3f> vertices, IReadOnlyList<Vector3f> normals, IReadOnlyList<ColorRGBA32> colors,
			IReadOnlyList<Vector2f> uv0, IReadOnlyList<Vector2f> uv1, IReadOnlyList<Vector4f> tangents)
		{
			BitArray curChannels = new BitArray(32);
			byte stride = 0;

			bool isWriteVertices = vertices.Count > 0;
			bool isWriteNormals = normals != null && normals.Count > 0;
			bool isWriteColors = colors.Count > 0;
			bool isWriteUV0 = uv0.Count > 0;
			bool isWriteUV1 = uv1 != null && uv1.Count > 0;
			bool isWriteTangents = tangents != null && tangents.Count > 0;

			if (isWriteVertices)
			{
				curChannels.Set((int)ChannelType4.Vertex, true);
				stride += ChannelType4.Vertex.GetStride();
			}
			if (isWriteNormals)
			{
				curChannels.Set((int)ChannelType4.Normal, true);
				stride += ChannelType4.Normal.GetStride();
			}
			if(isWriteColors)
			{
				curChannels.Set((int)ChannelType4.Color, true);
				stride += ChannelType4.Color.GetStride();
			}
			if (isWriteUV0)
			{
				curChannels.Set((int)ChannelType4.UV0, true);
				stride += ChannelType4.UV0.GetStride();
			}
			if (isWriteUV1)
			{
				curChannels.Set((int)ChannelType4.UV1, true);
				stride += ChannelType4.UV1.GetStride();
			}
			if (isWriteTangents)
			{
				curChannels.Set((int)ChannelType4.Tangent, true);
				stride += ChannelType4.Tangent.GetStride();
			}

			CurrentChannels = curChannels.ToUInt32();
			VertexCount = vertices.Count;
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

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 1;
			}

			if (version.IsGreaterEqual(2018))
			{
				return 2;
			}
			return 1;
		}

		public Vector3f[] GenerateVertices(Version version, SubMesh submesh)
		{
			IReadOnlyList<ChannelInfo> channels = GetChannels(version);
			ChannelInfo channel = channels[(int)ChannelType.Vertex];
			if (!channel.IsSet)
			{
				throw new Exception("Vertices hasn't been found");
			}

			Vector3f[] verts = new Vector3f[submesh.VertexCount];
			int vertexSize = channels.Where(t => t.Stream == channel.Stream).Sum(t => t.GetStride());
			int streamOffset = GetStreamOffset(channel.Stream, channels);
			using (MemoryStream memStream = new MemoryStream(m_data))
			{
				using (BinaryReader reader = new BinaryReader(memStream))
				{
					memStream.Position = streamOffset + submesh.FirstVertex * vertexSize + channel.Offset;
					for (int v = 0; v < submesh.VertexCount; v++)
					{
						float x = reader.ReadSingle();
						float y = reader.ReadSingle();
						float z = reader.ReadSingle();
						verts[v] = new Vector3f(x, y, z);
						memStream.Position += vertexSize - 12;
					}
				}
			}
			return verts;
		}

		public BoneWeights4[] GenerateSkin()
		{
			BoneWeights4[] skin = new BoneWeights4[VertexCount];
			ChannelInfo weightChannel = Channels[(int)ChannelType2018.SkinWeight];
			ChannelInfo boneIndexChannel = Channels[(int)ChannelType2018.SkinBoneIndex];
			if(!weightChannel.IsSet)
			{
				return new BoneWeights4[0];
			}

			int weightVertexSize = Channels.Where(t => t.Stream == weightChannel.Stream).Sum(t => t.GetStride());
			int weightStreamOffset = GetStreamOffset(weightChannel.Stream, Channels);
			int boneIndexVertexSize = Channels.Where(t => t.Stream == boneIndexChannel.Stream).Sum(t => t.GetStride());
			int boneIndexStreamOffset = GetStreamOffset(boneIndexChannel.Stream, Channels);
			using (MemoryStream memStream = new MemoryStream(m_data))
			{
				using (BinaryReader reader = new BinaryReader(memStream))
				{
					for (int v = 0; v < VertexCount; v++)
					{
						memStream.Position = weightStreamOffset + v * weightVertexSize + weightChannel.Offset;
						float w0 = reader.ReadSingle();
						float w1 = reader.ReadSingle();
						float w2 = reader.ReadSingle();
						float w3 = reader.ReadSingle();

						memStream.Position = boneIndexStreamOffset + v * boneIndexVertexSize + boneIndexChannel.Offset;
						int i0 = reader.ReadInt32();
						int i1 = reader.ReadInt32();
						int i2 = reader.ReadInt32();
						int i3 = reader.ReadInt32();

						skin[v] = new BoneWeights4(w0, w1, w2, w3, i0, i1, i2, i3);
					}
				}
			}
			return skin;
		}

		public void Read(AssetStream stream)
		{
			if(IsReadCurrentChannels(stream.Version))
			{
				CurrentChannels = stream.ReadUInt32();
			}
			VertexCount = (int)stream.ReadUInt32();

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

		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_CurrentChannels", GetCurrentChannels(container.Version));
			node.Add("m_VertexCount", VertexCount);
			node.Add("m_Channels", GetChannels(container.Version).ExportYAML(container));
			node.Add("m_DataSize", m_data.Length);
			node.Add("_typelessdata", m_data.ExportYAML());
			return node;
		}

		private uint GetCurrentChannels(Version version)
		{
			if(IsReadCurrentChannels(version))
			{
				if (IsReadStream(version))
				{
					BitArray curBits = CurrentChannelsBits;
					curBits.Set((int)ChannelType.Tangent, curBits.Get((int)ChannelType4.Tangent));
					curBits.Set((int)ChannelType4.Tangent, false);
					return curBits.ToUInt32();
				}
				else
				{
					return CurrentChannels;
				}
			}
			else
			{
				BitArray curChannels = new BitArray(32);
				for (int i = 0; i < Channels.Count; i++)
				{
					ChannelType2018 channelType = (ChannelType2018)i;
					switch(channelType)
					{
						case ChannelType2018.Vertex:
						case ChannelType2018.Normal:
						case ChannelType2018.Color:
						case ChannelType2018.UV0:
						case ChannelType2018.UV1:
						case ChannelType2018.UV2:
						case ChannelType2018.UV3:
						case ChannelType2018.Tangent:
							{
								ChannelInfo channel = Channels[i];
								curChannels[(int)channelType.ToChannelType()] |= channel.Dimension != 0;
							}
							break;
					}
				}
				return curChannels.ToUInt32();
			}
		}
		private IReadOnlyList<ChannelInfo> GetChannels(Version version)
		{
			if (IsReadChannels(version))
			{
				if(IsReadStream(version))
				{
					ChannelInfo[] channels = new ChannelInfo[8];
					channels[(int)ChannelType.Vertex] = Channels[(int)ChannelType4.Vertex];
					channels[(int)ChannelType.Normal] = Channels[(int)ChannelType4.Normal];
					channels[(int)ChannelType.Color] = Channels[(int)ChannelType4.Color];
					channels[(int)ChannelType.UV0] = Channels[(int)ChannelType4.UV0];
					channels[(int)ChannelType.UV1] = Channels[(int)ChannelType4.UV1];
					channels[(int)ChannelType.UV2] = new ChannelInfo(0, 0, 0, 0);
					channels[(int)ChannelType.UV3] = new ChannelInfo(0, 0, 0, 0);
					channels[(int)ChannelType.Tangent] = Channels[(int)ChannelType4.Tangent];
					return channels;
				}
				else if (IsReadCurrentChannels(version))
				{
					return Channels;
				}
				else
				{
					ChannelInfo[] channels = new ChannelInfo[8];
					channels[(int)ChannelType.Vertex] = Channels[(int)ChannelType2018.Vertex];
					channels[(int)ChannelType.Normal] = Channels[(int)ChannelType2018.Normal];
					channels[(int)ChannelType.Color] = Channels[(int)ChannelType2018.Color];
					channels[(int)ChannelType.UV0] = Channels[(int)ChannelType2018.UV0];
					channels[(int)ChannelType.UV1] = Channels[(int)ChannelType2018.UV1];
					channels[(int)ChannelType.UV2] = Channels[(int)ChannelType2018.UV2];
					channels[(int)ChannelType.UV3] = Channels[(int)ChannelType2018.UV3];
					channels[(int)ChannelType.Tangent] = Channels[(int)ChannelType2018.Tangent];
					return channels;
				}
			}
			else
			{
				ChannelInfo[] channels = new ChannelInfo[8];
				channels[(int)ChannelType.Vertex] = CreateChannelFromStream(ChannelType4.Vertex);
				channels[(int)ChannelType.Normal] = CreateChannelFromStream(ChannelType4.Normal);
				channels[(int)ChannelType.Color] = CreateChannelFromStream(ChannelType4.Color);
				channels[(int)ChannelType.UV0] = CreateChannelFromStream(ChannelType4.UV0);
				channels[(int)ChannelType.UV1] = CreateChannelFromStream(ChannelType4.UV1);
				channels[(int)ChannelType.UV2] = new ChannelInfo(0, 0, 0, 0);
				channels[(int)ChannelType.UV3] = new ChannelInfo(0, 0, 0, 0);
				channels[(int)ChannelType.Tangent] = CreateChannelFromStream(ChannelType4.Tangent);
				return channels;
			}
		}

		private BitArray CreateChannelsBits(uint channels)
		{
			return new BitArray(BitConverter.GetBytes(CurrentChannels));
		}

		private ChannelInfo CreateChannelFromStream(ChannelType4 channelType)
		{
			int streamIndex = Streams.IndexOf(t => t.IsMatch(channelType));
			if (streamIndex == -1)
			{
				return new ChannelInfo(0, 0, 0, 0);
			}
			else
			{
				StreamInfo stream = Streams[streamIndex];
				byte offset = 0;
				for (ChannelType4 type = ChannelType4.Vertex; type < channelType; type++)
				{
					if(stream.IsMatch(type))
					{
						offset += type.GetStride();
					}
				}
				return new ChannelInfo((byte)streamIndex, offset, channelType.GetFormat(), channelType.GetDimention());
			}
		}

		private int GetStreamOffset(int stream, IReadOnlyList<ChannelInfo> channels)
		{
			if (stream == 0)
			{
				return 0;
			}

			// There is a gape between streams (usually 8 bytes )
			// This is NOT an alignment, even if sometimes it may seem so
			int leftSize = channels.Where(t => t.Stream != 0).Sum(t => t.GetStride());
			int offset = m_data.Length - leftSize * VertexCount;

			for(int i = 1; i < stream; i++)
			{
				int vertexSize = channels.Where(t => t.Stream == i).Sum(t => t.GetStride());
				offset += vertexSize * VertexCount;
			}
			return offset;
		}

		public BitArray CurrentChannelsBits => CreateChannelsBits(CurrentChannels);

		public uint CurrentChannels { get; private set; }
		public int VertexCount { get; private set; }
		public IReadOnlyList<ChannelInfo> Channels => m_channels;
		public IReadOnlyList<StreamInfo> Streams => m_streams;
		public IReadOnlyList<byte> Data => m_data;

		private ChannelInfo[] m_channels;
		private StreamInfo[] m_streams;
		private byte[] m_data;
	}
}
