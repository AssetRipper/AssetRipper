﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Shader.Enums.ShaderChannel;
using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.SourceGenerated.Subclasses.ChannelInfo;
using AssetRipper.SourceGenerated.Subclasses.StreamInfo;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using AssetRipper.SourceGenerated.Subclasses.Vector3f;
using AssetRipper.SourceGenerated.Subclasses.VertexData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MeshHelper = AssetRipper.Core.Classes.Mesh.MeshHelper;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class VertexDataExtensions
	{
		private const int StaticStreamCount = 4;
		private const int VertexStreamAlign = 16;

		public static bool IsSet(this IVertexData instance) => instance.VertexCount > 0;

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
				return new();
				//return StreamInfoConverter.GenerateChannelInfo(version, instance.Streams, channelType);
			}
		}

		public static void ReadData(
			this IVertexData instance,
			UnityVersion version,
			out int vertexCount,
			out Vector3f[]? vertices,
			out Vector3f[]? normals,
			out Vector4f[]? tangents,
			out ColorRGBA32[]? colors,
			out BoneWeights4[]? skin,
			out Vector2f[]? uv0,
			out Vector2f[]? uv1,
			out Vector2f[]? uv2,
			out Vector2f[]? uv3,
			out Vector2f[]? uv4,
			out Vector2f[]? uv5,
			out Vector2f[]? uv6,
			out Vector2f[]? uv7)
		{
			vertexCount = (int)instance.VertexCount;

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

			AssetList<ChannelInfo> channels = instance.GetChannels(version);
			List<IStreamInfo> streams = instance.GetStreams(version);

			for (int chn = 0; chn < channels.Count; chn++)
			{
				SourceGenerated.Subclasses.ChannelInfo.ChannelInfo? m_Channel = channels[chn];
				if (m_Channel.GetDataDimension() > 0)
				{
					IStreamInfo m_Stream = streams[m_Channel.Stream];
					BitArray? channelMask = new BitArray(BitConverter.GetBytes(m_Stream.ChannelMask));
					if (channelMask.Get(chn))
					{
						if (version.IsLess(2018) && chn == 2 && m_Channel.Format == 2) //kShaderChannelColor && kChannelFormatColor
						{
							m_Channel.SetDataDimension(4);
						}

						MeshHelper.VertexFormat vertexFormat = MeshHelper.ToVertexFormat(m_Channel.Format, version);
						int componentByteSize = (int)MeshHelper.GetFormatSize(vertexFormat);
						byte[]? componentBytes = new byte[vertexCount * m_Channel.GetDataDimension() * componentByteSize];
						for (int v = 0; v < vertexCount; v++)
						{
							int vertexOffset = (int)m_Stream.Offset + m_Channel.Offset + (int)m_Stream.GetStride() * v;
							for (int d = 0; d < m_Channel.GetDataDimension(); d++)
							{
								int componentOffset = vertexOffset + componentByteSize * d;
								Buffer.BlockCopy(instance.Data, componentOffset, componentBytes, componentByteSize * (v * m_Channel.GetDataDimension() + d), componentByteSize);
							}
						}

						if (instance.EndianType == EndianType.BigEndian && componentByteSize > 1) //swap bytes
						{
							for (int i = 0; i < componentBytes.Length / componentByteSize; i++)
							{
								byte[]? buff = new byte[componentByteSize];
								Buffer.BlockCopy(componentBytes, i * componentByteSize, buff, 0, componentByteSize);
								buff = buff.Reverse().ToArray();
								Buffer.BlockCopy(buff, 0, componentBytes, i * componentByteSize, componentByteSize);
							}
						}

						int[]? componentsIntArray = null;
						float[]? componentsFloatArray = null;
						if (MeshHelper.IsIntFormat(vertexFormat))
							componentsIntArray = MeshHelper.BytesToIntArray(componentBytes, vertexFormat);
						else
							componentsFloatArray = MeshHelper.BytesToFloatArray(componentBytes, vertexFormat);

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
									colors = MeshHelper.FloatArrayToColorRGBA32(componentsFloatArray);
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
									if (skin == null)
									{
										skin = MakeInitializedArray<BoneWeights4>(vertexCount);
									}
									for (int i = 0; i < vertexCount; i++)
									{
										for (int j = 0; j < m_Channel.GetDataDimension(); j++)
										{
											skin[i].Weights[j] = componentsFloatArray[i * m_Channel.GetDataDimension() + j];
										}
									}
									break;
								case 13: //kShaderChannelBlendIndices
									if (skin == null)
									{
										skin = MakeInitializedArray<BoneWeights4>(vertexCount);
									}
									for (int i = 0; i < vertexCount; i++)
									{
										for (int j = 0; j < m_Channel.GetDataDimension(); j++)
										{
											skin[i].BoneIndices[j] = componentsIntArray[i * m_Channel.GetDataDimension() + j];
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
									colors = MeshHelper.FloatArrayToColorRGBA32(componentsFloatArray);
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

		public static BoneWeights4[] GenerateSkin(this IVertexData instance, IExportContainer container)
		{
			ChannelInfo weightChannel = instance.Channels[(int)ShaderChannel2018.SkinWeight];
			ChannelInfo indexChannel = instance.Channels[(int)ShaderChannel2018.SkinBoneIndex];
			if (!weightChannel.IsSet())
			{
				return Array.Empty<BoneWeights4>();
			}

			BoneWeights4[] skin = new BoneWeights4[instance.VertexCount];
			int weightStride = instance.Channels.Where(t => t.Stream == weightChannel.Stream).Sum(t => t.GetStride(container.Version));
			int weightStreamOffset = instance.GetStreamOffset(container.Version, weightChannel.Stream);
			int indexStride = instance.Channels.Where(t => t.Stream == indexChannel.Stream).Sum(t => t.GetStride(container.Version));
			int indexStreamOffset = instance.GetStreamOffset(container.Version, indexChannel.Stream);

			using MemoryStream memStream = new MemoryStream(instance.Data);
			using BinaryReader reader = new BinaryReader(memStream);

			int weightCount = System.Math.Min((int)weightChannel.GetDataDimension(), 4);
			int indexCount = System.Math.Min((int)indexChannel.GetDataDimension(), 4);
			float[] weights = new float[System.Math.Max(weightCount, 4)];
			int[] indices = new int[System.Math.Max(indexCount, 4)];
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

				skin[v] = new BoneWeights4(weights[0], weights[1], weights[2], weights[3], indices[0], indices[1], indices[2], indices[3]);
			}
			return skin;
		}

		public static Vector3f_3_5_0_f5[] GenerateVertices(this IVertexData instance, UnityVersion version, ISubMesh submesh)
		{
			IChannelInfo channel = instance.GetChannel(version, ShaderChannel.Vertex);
			if (!channel.IsSet())
			{
				if (AllowUnsetVertexChannel(version))
				{
					return Array.Empty<Vector3f_3_5_0_f5>();
				}
				else
				{
					throw new Exception("Vertices hasn't been found");
				}
			}

			Vector3f_3_5_0_f5[] verts = new Vector3f_3_5_0_f5[submesh.VertexCount];
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
					verts[v] = new();
					verts[v].SetValues(x, y, z);
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
				offset = (offset + (VertexStreamAlign - 1)) & ~(VertexStreamAlign - 1);
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
					var m_Channel = instance.Channels[chn];
					if (m_Channel.Stream == s)
					{
						if (m_Channel.GetDataDimension() > 0)
						{
							chnMask |= 1u << chn;
							stride += m_Channel.GetDataDimension() * MeshHelper.GetFormatSize(MeshHelper.ToVertexFormat(m_Channel.Format, version));
						}
					}
				}

				StreamInfo_4_0_0_f7 newStream = new StreamInfo_4_0_0_f7
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
				offset = (offset + (16u - 1u)) & ~(16u - 1u);
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
					instance.Streams_1_,
					instance.Streams_2_,
					instance.Streams_3_
				};
			}
			else
			{
				return new();
			}
		}

		private static AssetList<ChannelInfo> GetChannels(this IVertexData instance, UnityVersion version)
		{
			if (instance.Has_Channels())
			{
				return instance.Channels;
			}
			AssetList<ChannelInfo> channels = new AssetList<ChannelInfo>(6);
			List<IStreamInfo> streams = instance.GetStreamsInvariant();
			for (var s = 0; s < streams.Count; s++)
			{
				var m_Stream = streams[s];
				var channelMask = new BitArray(new[] { (int)m_Stream.ChannelMask });
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
