using System;
using System.IO;
using System.Linq;
using uTinyRipper.Converters;
using uTinyRipper.Converters.Meshes;
using uTinyRipper.YAML;
using uTinyRipper;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Classes.Shaders;

namespace uTinyRipper.Classes.Meshes
{
	public struct VertexData : IAsset
	{
		public static int ToSerializedVersion(Version version)
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
		public static bool HasCurrentChannels(Version version) => version.IsLess(2018);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasChannels(Version version) => version.IsGreaterEqual(4);
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasStreams(Version version) => version.IsLess(5);

		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool IsStreamStatic(Version version) => version.IsLess(4);

		/// <summary>
		/// 5.6.0
		/// </summary>
		private static bool AllowUnsetVertexChannel(Version version) => version.IsEqual(5, 6, 0);

		public ChannelInfo GetChannel(Version version, ShaderChannel channelType)
		{
			if (HasChannels(version))
			{
				return Channels[channelType.ToChannel(version)];
			}
			else
			{
				return StreamInfoConverter.GenerateChannelInfo(version, Streams, channelType);
			}
		}

		public BoneWeights4[] GenerateSkin(IExportContainer container)
		{
			ChannelInfo weightChannel = Channels[(int)ShaderChannel2018.SkinWeight];
			ChannelInfo indexChannel = Channels[(int)ShaderChannel2018.SkinBoneIndex];
			if (!weightChannel.IsSet)
			{
				return Array.Empty<BoneWeights4>();
			}

			BoneWeights4[] skin = new BoneWeights4[VertexCount];
			int weightStride = Channels.Where(t => t.Stream == weightChannel.Stream).Sum(t => t.GetStride(container.Version));
			int weightStreamOffset = GetStreamOffset(container.Version, weightChannel.Stream);
			int indexStride = Channels.Where(t => t.Stream == indexChannel.Stream).Sum(t => t.GetStride(container.Version));
			int indexStreamOffset = GetStreamOffset(container.Version, indexChannel.Stream);
			using (MemoryStream memStream = new MemoryStream(Data))
			{
				using (BinaryReader reader = new BinaryReader(memStream))
				{
					int weightCount = Math.Min((int)weightChannel.Dimension, 4);
					int indexCount = Math.Min((int)indexChannel.Dimension, 4);
					float[] weights = new float[Math.Max(weightCount, 4)];
					int[] indices = new int[Math.Max(indexCount, 4)];
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
				}
			}
			return skin;
		}

		public Vector3f[] GenerateVertices(Version version, ref SubMesh submesh)
		{
			ChannelInfo channel = GetChannel(version, ShaderChannel.Vertex);
			if (!channel.IsSet)
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
			int streamStride = Channels.Where(t => t.Stream == channel.Stream).Sum(t => t.GetStride(version));
			int streamOffset = GetStreamOffset(version, channel.Stream);
			using (MemoryStream memStream = new MemoryStream(Data))
			{
				using (BinaryReader reader = new BinaryReader(memStream))
				{
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
			}
			return verts;
		}

		public void Read(AssetReader reader)
		{
			if (HasCurrentChannels(reader.Version))
			{
				CurrentChannels = reader.ReadUInt32();
			}
			VertexCount = (int)reader.ReadUInt32();

			if (HasChannels(reader.Version))
			{
				Channels = reader.ReadAssetArray<ChannelInfo>();
				reader.AlignStream();
			}
			if (HasStreams(reader.Version))
			{
				if (IsStreamStatic(reader.Version))
				{
					Streams = new StreamInfo[StaticStreamCount];
					for (int i = 0; i < StaticStreamCount; i++)
					{
						Streams[i] = reader.ReadAsset<StreamInfo>();
					}
				}
				else
				{
					Streams = reader.ReadAssetArray<StreamInfo>();
				}
			}

			Data = reader.ReadByteArray();
			reader.AlignStream();
		}

		public void Write(AssetWriter writer)
		{
			if (HasCurrentChannels(writer.Version))
			{
				writer.Write(CurrentChannels);
			}
			writer.Write(VertexCount);

			if (HasChannels(writer.Version))
			{
				Channels.Write(writer);
				writer.AlignStream();
			}
			if (HasStreams(writer.Version))
			{
				if (IsStreamStatic(writer.Version))
				{
					for (int i = 0; i < StaticStreamCount; i++)
					{
						writer.WriteAsset(Streams[i]);
					}
				}
				else
				{
					Channels.Write(writer);
				}
			}

			Data.Write(writer);
			writer.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
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
				node.Add(ChannelsName, Channels.ExportYAML(container));
			}
			if (HasStreams(container.ExportVersion))
			{
				if (IsStreamStatic(container.ExportVersion))
				{
					for (int i = 0; i < StaticStreamCount; i++)
					{
						node.Add($"{StreamsName}[{i}]", Streams[i].ExportYAML(container));
					}
				}
				else
				{
					node.Add(StreamsName, Streams.ExportYAML(container));
				}
			}

			node.Add(DataSizeName, Data.Length);
			node.Add(container.Layout.TypelessdataName, Data.ExportYAML());
			return node;
		}

		public int GetStreamStride(Version version, int stream)
		{
			return HasStreams(version) ?
				(int)Streams[stream].Stride : Channels.Where(t => t.IsSet && t.Stream == stream).Sum(t => t.GetStride(version));
		}

		public int GetStreamSize(Version version, int stream)
		{
			return GetStreamStride(version, stream) * VertexCount;
		}

		public int GetStreamOffset(Version version, int stream)
		{
			int offset = 0;
			for (int i = 0; i < stream; i++)
			{
				offset += GetStreamSize(version, i);
				offset = (offset + (VertexStreamAlign - 1)) & ~(VertexStreamAlign - 1);
			}
			return offset;
		}

		public bool IsSet => VertexCount > 0;

		public uint CurrentChannels { get; set; }
		public int VertexCount { get; set; }
		public ChannelInfo[] Channels { get; set; }
		public StreamInfo[] Streams { get; set; }
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
