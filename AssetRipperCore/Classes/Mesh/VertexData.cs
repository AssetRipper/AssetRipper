using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Shader.Enums.ShaderChannel;
using AssetRipper.Core.Classes.Shader.Enums.VertexFormat;
using AssetRipper.Core.Converters.Mesh;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Core.Utils;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;
using System.Collections;
using System.IO;
using System.Linq;


namespace AssetRipper.Core.Classes.Mesh
{
	public sealed class VertexData : UnityAssetBase, IVertexData
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			// VertexFormat enum has been changed
			if (VertexFormatExtensions.VertexFormat2019Relevant(version))
			{
				return 3;
			}
			// ShaderChannel enum has been changed
			if (ShaderChannelExtensions.ShaderChannel2018Relevant(version))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 2018.1
		/// </summary>
		public static bool HasCurrentChannels(UnityVersion version) => version.IsLess(2018);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasChannels(UnityVersion version) => version.IsGreaterEqual(4);
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasStreams(UnityVersion version) => version.IsLess(5);

		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool IsStreamStatic(UnityVersion version) => version.IsLess(4);

		/// <summary>
		/// 5.6.0
		/// </summary>
		private static bool AllowUnsetVertexChannel(UnityVersion version) => version.IsEqual(5, 6, 0);

		public ChannelInfo GetChannel(UnityVersion version, ShaderChannel channelType)
		{
			if (HasChannels(version))
			{
				return m_Channels[channelType.ToChannel(version)];
			}
			else
			{
				return StreamInfoConverter.GenerateChannelInfo(version, m_Streams, channelType);
			}
		}

		public BoneWeights4[] GenerateSkin(IExportContainer container)
		{
			ChannelInfo weightChannel = m_Channels[(int)ShaderChannel2018.SkinWeight];
			ChannelInfo indexChannel = m_Channels[(int)ShaderChannel2018.SkinBoneIndex];
			if (!weightChannel.IsSet())
			{
				return Array.Empty<BoneWeights4>();
			}

			BoneWeights4[] skin = new BoneWeights4[VertexCount];
			int weightStride = m_Channels.Where(t => t.Stream == weightChannel.Stream).Sum(t => t.GetStride(container.Version));
			int weightStreamOffset = GetStreamOffset(container.Version, weightChannel.Stream);
			int indexStride = m_Channels.Where(t => t.Stream == indexChannel.Stream).Sum(t => t.GetStride(container.Version));
			int indexStreamOffset = GetStreamOffset(container.Version, indexChannel.Stream);

			using MemoryStream memStream = new MemoryStream(Data);
			using BinaryReader reader = new BinaryReader(memStream);

			int weightCount = System.Math.Min((int)weightChannel.GetDataDimension(), 4);
			int indexCount = System.Math.Min((int)indexChannel.GetDataDimension(), 4);
			float[] weights = new float[System.Math.Max(weightCount, 4)];
			int[] indices = new int[System.Math.Max(indexCount, 4)];
			for (int v = 0; v < VertexCount; v++)
			{
				memStream.Position = weightStreamOffset + v * weightStride + weightChannel.Offset;
				for (int i = 0; i < weightCount; i++)
				{
					weights[i] = reader.ReadSingle();
				}

				memStream.Position = indexStreamOffset + v * indexStride + indexChannel.Offset;
				for (int i = 0; i < indexCount; i++)
				{
					indices[i] = reader.ReadInt32();
				}

				skin[v] = new BoneWeights4(weights[0], weights[1], weights[2], weights[3], indices[0], indices[1], indices[2], indices[3]);
			}
			return skin;
		}

		public Vector3f[] GenerateVertices(UnityVersion version, ISubMesh submesh)
		{
			ChannelInfo channel = GetChannel(version, ShaderChannel.Vertex);
			if (!channel.IsSet())
			{
				if (AllowUnsetVertexChannel(version))
				{
					return Array.Empty<Vector3f>();
				}
				else
				{
					throw new Exception("Vertices hasn't been found");
				}
			}

			Vector3f[] verts = new Vector3f[submesh.VertexCount];
			int streamStride = m_Channels.Where(t => t.Stream == channel.Stream).Sum(t => t.GetStride(version));
			int streamOffset = GetStreamOffset(version, channel.Stream);
			using (MemoryStream memStream = new MemoryStream(Data))
			{
				using BinaryReader reader = new BinaryReader(memStream);
				memStream.Position = streamOffset + submesh.FirstVertex * streamStride + channel.Offset;
				for (int v = 0; v < submesh.VertexCount; v++)
				{
					float x = reader.ReadSingle();
					float y = reader.ReadSingle();
					float z = reader.ReadSingle();
					verts[v] = new Vector3f(x, y, z);
					memStream.Position += streamStride - 12;
				}
			}
			return verts;
		}

		public override void Read(AssetReader reader)
		{
			if (HasCurrentChannels(reader.Version))
			{
				CurrentChannels = reader.ReadUInt32();
			}
			VertexCount = reader.ReadUInt32();

			if (HasChannels(reader.Version)) // 4 and greater
			{
				m_Channels = reader.ReadAssetArray<ChannelInfo>();
				reader.AlignStream();
			}

			if (HasStreams(reader.Version)) // less than 5
			{
				if (IsStreamStatic(reader.Version)) // less than 4
				{
					m_Streams = new StreamInfo[StaticStreamCount];
					for (int i = 0; i < StaticStreamCount; i++)
					{
						m_Streams[i] = reader.ReadAsset<StreamInfo>();
					}
				}
				else // 4.x.x
				{
					m_Streams = reader.ReadAssetArray<StreamInfo>();
				}
			}
			else //5 and higher
			{
				//Uses channels to get streams
				GetStreams(reader.Version);
			}

			if (!HasChannels(reader.Version)) //less than 4
			{
				//Uses streams to get channels
				GetChannels(reader.Version);
			}

			Data = reader.ReadByteArray();
			reader.AlignStream();
		}

		public override void Write(AssetWriter writer)
		{
			if (HasCurrentChannels(writer.Version))
			{
				writer.Write(CurrentChannels);
			}
			writer.Write(VertexCount);

			if (HasChannels(writer.Version))
			{
				m_Channels.Write(writer);
				writer.AlignStream();
			}
			if (HasStreams(writer.Version))
			{
				if (IsStreamStatic(writer.Version))
				{
					for (int i = 0; i < StaticStreamCount; i++)
					{
						writer.WriteAsset(m_Streams[i]);
					}
				}
				else
				{
					m_Channels.Write(writer);
				}
			}

			Data.Write(writer);
			writer.AlignStream();
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			if (HasCurrentChannels(container.ExportVersion))
			{
				node.Add(CurrentChannelsName, CurrentChannels);
			}
			node.Add(VertexCountName, VertexCount);

			if (HasChannels(container.ExportVersion))
			{
				node.Add(ChannelsName, m_Channels.ExportYAML(container));
			}
			if (HasStreams(container.ExportVersion))
			{
				if (IsStreamStatic(container.ExportVersion))
				{
					for (int i = 0; i < StaticStreamCount; i++)
					{
						node.Add($"{StreamsName}[{i}]", m_Streams[i].ExportYAML(container));
					}
				}
				else
				{
					node.Add(StreamsName, m_Streams.ExportYAML(container));
				}
			}

			node.Add(DataSizeName, Data.Length);
			node.Add(Layout.LayoutInfo.TypelessdataName, Data.ExportYAML());
			return node;
		}

		public int GetStreamStride(UnityVersion version, int stream)
		{
			return HasStreams(version) ?
				(int)m_Streams[stream].Stride : m_Channels.Where(t => t.IsSet() && t.Stream == stream).Sum(t => t.GetStride(version));
		}

		public int GetStreamSize(UnityVersion version, int stream)
		{
			return GetStreamStride(version, stream) * (int)VertexCount;
		}

		public int GetStreamOffset(UnityVersion version, int stream)
		{
			int offset = 0;
			for (int i = 0; i < stream; i++)
			{
				offset += GetStreamSize(version, i);
				offset = (offset + (VertexStreamAlign - 1)) & ~(VertexStreamAlign - 1);
			}
			return offset;
		}

		private void GetStreams(UnityVersion version)
		{
			var streamCount = m_Channels.Max(x => x.Stream) + 1;
			m_Streams = new StreamInfo[streamCount];
			long offset = 0;
			for (int s = 0; s < streamCount; s++)
			{
				uint chnMask = 0;
				uint stride = 0;
				for (int chn = 0; chn < m_Channels.Length; chn++)
				{
					var m_Channel = m_Channels[chn];
					if (m_Channel.Stream == s)
					{
						if (m_Channel.GetDataDimension() > 0)
						{
							chnMask |= 1u << chn;
							stride += m_Channel.GetDataDimension() * MeshHelper.GetFormatSize(MeshHelper.ToVertexFormat(m_Channel.Format, version));
						}
					}
				}
				m_Streams[s] = new StreamInfo
				{
					ChannelMask = chnMask,
					Offset = (uint)offset,
					Stride = stride,
					DividerOp = 0,
					Frequency = 0
				};
				offset += VertexCount * stride;
				//static size_t AlignStreamSize (size_t size) { return (size + (kVertexStreamAlign-1)) & ~(kVertexStreamAlign-1); }
				offset = (offset + (16u - 1u)) & ~(16u - 1u);
			}
		}

		private void GetChannels(UnityVersion version)
		{
			m_Channels = ArrayUtils.CreateAndInitializeArray<ChannelInfo>(6);
			for (var s = 0; s < m_Streams.Length; s++)
			{
				var m_Stream = m_Streams[s];
				var channelMask = new BitArray(new[] { (int)m_Stream.ChannelMask });
				byte offset = 0;
				for (int i = 0; i < 6; i++)
				{
					if (channelMask.Get(i))
					{
						ChannelInfo m_Channel = m_Channels[i];
						m_Channel.Stream = (byte)s;
						m_Channel.Offset = offset;
						switch (i)
						{
							case 0: //kShaderChannelVertex
							case 1: //kShaderChannelNormal
								m_Channel.Format = 0; //kChannelFormatFloat
								m_Channel.SetDataDimension(3);
								break;
							case 2: //kShaderChannelColor
								m_Channel.Format = 2; //kChannelFormatColor
								m_Channel.SetDataDimension(4);
								break;
							case 3: //kShaderChannelTexCoord0
							case 4: //kShaderChannelTexCoord1
								m_Channel.Format = 0; //kChannelFormatFloat
								m_Channel.SetDataDimension(2);
								break;
							case 5: //kShaderChannelTangent
								m_Channel.Format = 0; //kChannelFormatFloat
								m_Channel.SetDataDimension(4);
								break;
						}
						offset += (byte)(m_Channel.GetDataDimension() * MeshHelper.GetFormatSize(MeshHelper.ToVertexFormat(m_Channel.Format, version)));
					}
				}
			}
		}

		public uint CurrentChannels { get; set; }
		public uint VertexCount { get; set; }
		public ChannelInfo[] m_Channels { get; set; }
		public IChannelInfo[] Channels => m_Channels;
		public StreamInfo[] m_Streams { get; set; }
		public IStreamInfo[] Streams => m_Streams;
		public byte[] Data { get; set; }

		public const string CurrentChannelsName = "m_CurrentChannels";
		public const string VertexCountName = "m_VertexCount";
		public const string ChannelsName = "m_Channels";
		public const string StreamsName = "m_Streams";
		public const string DataSizeName = "m_DataSize";

		private const int StaticStreamCount = 4;
		private const int VertexStreamAlign = 16;
	}
}
