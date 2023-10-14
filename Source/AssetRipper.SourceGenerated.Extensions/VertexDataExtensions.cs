using AssetRipper.Assets.Export;
using AssetRipper.Assets.Generics;
using AssetRipper.IO.Endian;
using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.ShaderChannel;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.VertexFormat;
using AssetRipper.SourceGenerated.Subclasses.ChannelInfo;
using AssetRipper.SourceGenerated.Subclasses.StreamInfo;
using AssetRipper.SourceGenerated.Subclasses.StreamingInfo;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using AssetRipper.SourceGenerated.Subclasses.VertexData;
using System.Collections;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class VertexDataExtensions
	{
		private const int StaticStreamCount = 4;
		private const int VertexStreamAlign = 16;

		public static bool IsSet(this IVertexData instance, IStreamingInfo? streamingInfo)
		{
			return instance.VertexCount > 0 && (instance.Data.Length > 0 || streamingInfo is not null && streamingInfo.IsSet());
		}

		/// <summary>
		/// 5.6.0
		/// </summary>
		private static bool AllowUnsetVertexChannel(UnityVersion version) => version.IsEqual(5, 6, 0);

		public static ChannelInfo GetChannel(this IVertexData instance, UnityVersion version, ShaderChannel channelType)
		{
			if (instance.Has_Channels())
			{
				return instance.Channels[channelType.ToChannel(version)];
			}
			else
			{
				List<IStreamInfo> streams = instance.GetStreamsInvariant();
				ChannelInfo channelInfo = new ChannelInfo();
				ShaderChannel4 channelv4 = channelType.ToShaderChannel4();
				int streamIndex = streams.IndexOf(t => t.IsMatch(channelv4));
				if (streamIndex >= 0)
				{
					byte offset = 0;
					IStreamInfo stream = streams[streamIndex];
					for (ShaderChannel4 i = 0; i < channelv4; i++)
					{
						if (stream.IsMatch(i))
						{
							offset += i.ToShaderChannel().GetStride(version);
						}
					}

					channelInfo.Stream = (byte)streamIndex;
					channelInfo.Offset = offset;
					channelInfo.Format = channelType.GetVertexFormat(version).ToFormat(version);
					channelInfo.Dimension = channelType.GetDimention(version);
				}
				return channelInfo;
			}
		}

		public static void ReadData(
			this IVertexData instance,
			UnityVersion version,
			EndianType endianType,
			IMesh? mesh,
			out Vector3[]? vertices,
			out Vector3[]? normals,
			out Vector4[]? tangents,
			out ColorFloat[]? colors,
			out BoneWeight4[]? skin,
			out Vector2[]? uv0,
			out Vector2[]? uv1,
			out Vector2[]? uv2,
			out Vector2[]? uv3,
			out Vector2[]? uv4,
			out Vector2[]? uv5,
			out Vector2[]? uv6,
			out Vector2[]? uv7)
		{
			int vertexCount = (int)instance.VertexCount;

			vertices = default;
			normals = default;
			tangents = default;
			colors = default;
			skin = default;
			uv0 = default;
			uv1 = default;
			uv2 = default;
			uv3 = default;
			uv4 = default;
			uv5 = default;
			uv6 = default;
			uv7 = default;

			byte[] data;

			if (instance.Data.Length == 0)
			{
				if (mesh?.StreamData is not null)
				{
					data = mesh.StreamData.GetContent(mesh.Collection);
					if (data.Length == 0)
					{
						return;
					}
				}
				else
				{
					return;
				}
			}
			else
			{
				data = instance.Data;
			}

			IReadOnlyList<ChannelInfo> channels = instance.GetChannels(version);
			List<IStreamInfo> streams = instance.GetStreams(version);

			for (int chn = 0; chn < channels.Count; chn++)
			{
				ChannelInfo m_Channel = channels[chn];
				if (m_Channel.GetDataDimension() > 0)
				{
					IStreamInfo m_Stream = streams[m_Channel.Stream];
					BitArray channelMask = new BitArray(BitConverter.GetBytes(m_Stream.ChannelMask));
					if (channelMask.Get(chn))
					{
						if (version.IsLess(2018) && chn == 2 && m_Channel.Format == 2) //kShaderChannelColor && kChannelFormatColor
						{
							m_Channel.SetDataDimension(4);
						}

						MeshHelper.VertexFormat vertexFormat = MeshHelper.ToVertexFormat(m_Channel.Format, version);
						int componentByteSize = MeshHelper.GetFormatSize(vertexFormat);
						byte[] componentBytes = new byte[vertexCount * m_Channel.GetDataDimension() * componentByteSize];
						for (int v = 0; v < vertexCount; v++)
						{
							int vertexOffset = (int)m_Stream.Offset + m_Channel.Offset + (int)m_Stream.GetStride() * v;
							for (int d = 0; d < m_Channel.GetDataDimension(); d++)
							{
								int componentOffset = vertexOffset + componentByteSize * d;
								Buffer.BlockCopy(data, componentOffset, componentBytes, componentByteSize * (v * m_Channel.GetDataDimension() + d), componentByteSize);
							}
						}

						if (endianType == EndianType.BigEndian && componentByteSize > 1) //swap bytes
						{
							for (int i = 0; i < componentBytes.Length / componentByteSize; i++)
							{
								byte[] buff = new byte[componentByteSize];
								Buffer.BlockCopy(componentBytes, i * componentByteSize, buff, 0, componentByteSize);
								buff = buff.Reverse().ToArray();
								Buffer.BlockCopy(buff, 0, componentBytes, i * componentByteSize, componentByteSize);
							}
						}

						int[] componentsIntArray = Array.Empty<int>();
						float[] componentsFloatArray = Array.Empty<float>();
						if (MeshHelper.IsIntFormat(vertexFormat))
						{
							componentsIntArray = MeshHelper.BytesToIntArray(componentBytes, vertexFormat);
						}
						else
						{
							componentsFloatArray = MeshHelper.BytesToFloatArray(componentBytes, vertexFormat);
						}

						if (version.IsGreaterEqual(2018))
						{
							switch (chn)
							{
								case 0: //kShaderChannelVertex
									vertices = MeshHelper.FloatArrayToVector3(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 1: //kShaderChannelNormal
									normals = MeshHelper.FloatArrayToVector3(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 2: //kShaderChannelTangent
									tangents = MeshHelper.FloatArrayToVector4(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 3: //kShaderChannelColor
									colors = MeshHelper.FloatArrayToColorFloat(componentsFloatArray);
									break;
								case 4: //kShaderChannelTexCoord0
									uv0 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 5: //kShaderChannelTexCoord1
									uv1 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 6: //kShaderChannelTexCoord2
									uv2 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 7: //kShaderChannelTexCoord3
									uv3 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 8: //kShaderChannelTexCoord4
									uv4 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 9: //kShaderChannelTexCoord5
									uv5 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 10: //kShaderChannelTexCoord6
									uv6 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 11: //kShaderChannelTexCoord7
									uv7 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								//2018.2 and up
								case 12: //kShaderChannelBlendWeight
									skin ??= MakeInitializedArray<BoneWeight4>(vertexCount);
									for (int i = 0; i < vertexCount; i++)
									{
										for (int j = 0; j < m_Channel.GetDataDimension(); j++)
										{
											BoneWeight4 boneWeight = skin[i];
											boneWeight.SetWeight(j, componentsFloatArray[i * m_Channel.GetDataDimension() + j]);
											skin[i] = boneWeight;
										}
									}
									break;
								case 13: //kShaderChannelBlendIndices
									skin ??= MakeInitializedArray<BoneWeight4>(vertexCount);
									for (int i = 0; i < vertexCount; i++)
									{
										for (int j = 0; j < m_Channel.GetDataDimension(); j++)
										{
											BoneWeight4 boneWeight = skin[i];
											boneWeight.SetIndex(j, componentsIntArray[i * m_Channel.GetDataDimension() + j]);
											skin[i] = boneWeight;
										}
									}
									break;
							}
						}
						else
						{
							switch (chn)
							{
								case 0: //kShaderChannelVertex
									vertices = MeshHelper.FloatArrayToVector3(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 1: //kShaderChannelNormal
									normals = MeshHelper.FloatArrayToVector3(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 2: //kShaderChannelColor
									colors = MeshHelper.FloatArrayToColorFloat(componentsFloatArray);
									break;
								case 3: //kShaderChannelTexCoord0
									uv0 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 4: //kShaderChannelTexCoord1
									uv1 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 5:
									if (version.IsGreaterEqual(5)) //kShaderChannelTexCoord2
									{
										uv2 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									}
									else //kShaderChannelTangent
									{
										tangents = MeshHelper.FloatArrayToVector4(componentsFloatArray, m_Channel.GetDataDimension());
									}
									break;
								case 6: //kShaderChannelTexCoord3
									uv3 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 7: //kShaderChannelTangent
									tangents = MeshHelper.FloatArrayToVector4(componentsFloatArray, m_Channel.GetDataDimension());
									break;
							}
						}
					}
				}
			}
		}

		public static BoneWeight4[] GenerateSkin(this IVertexData instance, IExportContainer container)
		{
			ChannelInfo weightChannel = instance.Channels[(int)ShaderChannel2018.SkinWeight];
			ChannelInfo indexChannel = instance.Channels[(int)ShaderChannel2018.SkinBoneIndex];
			if (!weightChannel.IsSet())
			{
				return Array.Empty<BoneWeight4>();
			}

			BoneWeight4[] skin = new BoneWeight4[instance.VertexCount];
			int weightStride = instance.Channels.Where(t => t.Stream == weightChannel.Stream).Sum(t => t.GetStride(container.Version));
			int weightStreamOffset = instance.GetStreamOffset(container.Version, weightChannel.Stream);
			int indexStride = instance.Channels.Where(t => t.Stream == indexChannel.Stream).Sum(t => t.GetStride(container.Version));
			int indexStreamOffset = instance.GetStreamOffset(container.Version, indexChannel.Stream);

			using MemoryStream memStream = new MemoryStream(instance.Data);
			using BinaryReader reader = new BinaryReader(memStream);

			int weightCount = Math.Min((int)weightChannel.GetDataDimension(), 4);
			int indexCount = Math.Min((int)indexChannel.GetDataDimension(), 4);
			float[] weights = new float[Math.Max(weightCount, 4)];
			int[] indices = new int[Math.Max(indexCount, 4)];
			for (int v = 0; v < instance.VertexCount; v++)
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

				skin[v] = new BoneWeight4(weights[0], weights[1], weights[2], weights[3], indices[0], indices[1], indices[2], indices[3]);
			}
			return skin;
		}

		public static Vector3[] GenerateVertices(this IVertexData instance, UnityVersion version, ISubMesh submesh)
		{
			IChannelInfo channel = instance.GetChannel(version, ShaderChannel.Vertex);
			if (!channel.IsSet())
			{
				if (AllowUnsetVertexChannel(version))
				{
					return Array.Empty<Vector3>();
				}
				else
				{
					throw new Exception("Vertices hasn't been found");
				}
			}

			Vector3[] verts = new Vector3[submesh.VertexCount];
			int streamStride = instance.Channels.Where(t => t.Stream == channel.Stream).Sum(t => t.GetStride(version));
			int streamOffset = instance.GetStreamOffset(version, channel.Stream);
			using (MemoryStream memStream = new MemoryStream(instance.Data))
			{
				using BinaryReader reader = new BinaryReader(memStream);
				memStream.Position = streamOffset + submesh.FirstVertex * streamStride + channel.Offset;
				for (int v = 0; v < submesh.VertexCount; v++)
				{
					float x = reader.ReadSingle();
					float y = reader.ReadSingle();
					float z = reader.ReadSingle();
					verts[v] = new Vector3(x, y, z);
					memStream.Position += streamStride - 12;
				}
			}
			return verts;
		}

		public static int GetStreamStride(this IVertexData instance, UnityVersion version, int stream)
		{
			return instance.Has_Streams() ?
				(int)instance.Streams[stream].GetStride() : instance.Channels.Where(t => t.IsSet() && t.Stream == stream).Sum(t => t.GetStride(version));
		}

		public static int GetStreamSize(this IVertexData instance, UnityVersion version, int stream)
		{
			return instance.GetStreamStride(version, stream) * (int)instance.VertexCount;
		}

		public static int GetStreamOffset(this IVertexData instance, UnityVersion version, int stream)
		{
			int offset = 0;
			for (int i = 0; i < stream; i++)
			{
				offset += instance.GetStreamSize(version, i);
				offset = offset + (VertexStreamAlign - 1) & ~(VertexStreamAlign - 1);
			}
			return offset;
		}

		private static List<IStreamInfo> GetStreams(this IVertexData instance, UnityVersion version)
		{
			if (instance.HasStreamsInvariant())
			{
				return instance.GetStreamsInvariant();
			}
			int streamCount = instance.Channels.Max(x => x.Stream) + 1;
			List<IStreamInfo> streams = new List<IStreamInfo>(streamCount);
			long offset = 0;
			for (int s = 0; s < streamCount; s++)
			{
				uint chnMask = 0;
				uint stride = 0;
				for (int chn = 0; chn < instance.Channels.Count; chn++)
				{
					ChannelInfo m_Channel = instance.Channels[chn];
					if (m_Channel.Stream == s)
					{
						if (m_Channel.GetDataDimension() > 0)
						{
							chnMask |= 1u << chn;
							stride += m_Channel.GetDataDimension() * (uint)MeshHelper.GetFormatSize(MeshHelper.ToVertexFormat(m_Channel.Format, version));
						}
					}
				}

				StreamInfo_4_0_0 newStream = new()
				{
					ChannelMask = chnMask,
					Offset = (uint)offset,
					Stride_Byte = (byte)stride,
					DividerOp = 0,
					Frequency = 0
				};
				streams.Add(newStream);

				offset += instance.VertexCount * stride;
				//static size_t AlignStreamSize (size_t size) { return (size + (kVertexStreamAlign-1)) & ~(kVertexStreamAlign-1); }
				offset = offset + (16u - 1u) & ~(16u - 1u);
			}
			return streams;
		}

		private static bool HasStreamsInvariant(this IVertexData instance) => instance.Has_Streams() || instance.Has_Streams_0_();

		private static List<IStreamInfo> GetStreamsInvariant(this IVertexData instance)
		{
			if (instance.Has_Streams())
			{
				return instance.Streams.Select(s => (IStreamInfo)s).ToList();
			}
			else if (instance.Has_Streams_0_())
			{
				return new List<IStreamInfo>()
				{
					instance.Streams_0_,
					instance.Streams_1_!,
					instance.Streams_2_!,
					instance.Streams_3_!
				};
			}
			else
			{
				return new();
			}
		}

		private static IReadOnlyList<ChannelInfo> GetChannels(this IVertexData instance, UnityVersion version)
		{
			if (instance.Has_Channels())
			{
				return instance.Channels;
			}
			AssetList<ChannelInfo> channels = new AssetList<ChannelInfo>(6);
			List<IStreamInfo> streams = instance.GetStreamsInvariant();
			for (int s = 0; s < streams.Count; s++)
			{
				IStreamInfo m_Stream = streams[s];
				BitArray channelMask = new BitArray(new[] { (int)m_Stream.ChannelMask });
				byte offset = 0;
				for (int i = 0; i < 6; i++)
				{
					if (channelMask.Get(i))
					{
						ChannelInfo m_Channel = channels.AddNew();
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
			return channels;
		}

		private static T[] MakeInitializedArray<T>(int size) where T : new()
		{
			T[] array = new T[size];
			for (int i = 0; i < size; i++)
			{
				array[i] = new();
			}
			return array;
		}
	}
}
