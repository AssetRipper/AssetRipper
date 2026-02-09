using AssetRipper.IO.Endian;
using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Subclasses.ChannelInfo;
using AssetRipper.SourceGenerated.Subclasses.StreamInfo;
using AssetRipper.SourceGenerated.Subclasses.VertexData;
using System.Buffers.Binary;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions;

public readonly record struct VertexDataBlob(
	IReadOnlyList<ChannelInfo> Channels,
	IReadOnlyList<IStreamInfo> Streams,
	byte[] Data,
	int VertexCount,
	UnityVersion Version,
	EndianType EndianType)
{
	private const int VertexStreamAlign = 16;

	public static VertexDataBlob Create(IMesh mesh)
	{
		return Create(mesh.VertexData, mesh.Collection.Version, mesh.Collection.EndianType, mesh.GetVertexDataBytes());
	}

	public static VertexDataBlob Create(IVertexData vertexData, UnityVersion version, EndianType endianType)
	{
		return Create(vertexData, version, endianType, vertexData.Data);
	}

	private static VertexDataBlob Create(IVertexData vertexData, UnityVersion version, EndianType endianType, byte[] data)
	{
		IReadOnlyList<ChannelInfo> channels;
		IReadOnlyList<IStreamInfo> streams;
		if (vertexData.Has_Channels())
		{
			channels = vertexData.Channels;
			streams = vertexData.GetStreams() ?? ConvertChannelsToStreams(vertexData.Channels, vertexData.VertexCount, version);
		}
		else
		{
			streams = vertexData.GetStreams()!;
			channels = ConvertStreamsToChannels(streams, version);
		}
		return new VertexDataBlob(channels, streams, data, (int)vertexData.VertexCount, version, endianType);
	}

	public static VertexDataBlob Create(MeshData meshData, UnityVersion version, EndianType endianType)
	{
		if (meshData.Vertices.Length == 0)
		{
			return new VertexDataBlob([], [], [], 0, version, endianType);
		}

		byte[] data;
		List<ChannelInfo> channels = new List<ChannelInfo>();
		{
			using MemoryStream memoryStream = new();
			using EndianWriter writer = new(memoryStream, endianType);

			if (version.GreaterThanOrEquals(2018))
			{
				//kShaderChannelVertex
				channels.Add(CreateChannelInfoForFloatVector(3, 0, version));
				Write(writer, meshData.Vertices);
				WriteAlignmentBytes(writer);

				//kShaderChannelNormal
				if (meshData.HasNormals)
				{
					// Maybe this should be dimension 4? I've seen models with 4D normals. The W component is always 0.
					channels.Add(CreateChannelInfoForFloatVector(3, 1, version));
					Write(writer, meshData.Normals);
					WriteAlignmentBytes(writer);
				}
				else
				{
					channels.Add(CreateChannelInfoForFloatVector(0, 1, version));
				}

				//kShaderChannelTangent
				if (meshData.HasTangents)
				{
					channels.Add(CreateChannelInfoForFloatVector(4, 2, version));
					Write(writer, meshData.Tangents);
					WriteAlignmentBytes(writer);
				}
				else
				{
					channels.Add(CreateChannelInfoForFloatVector(0, 2, version));
				}

				//kShaderChannelColor
				if (meshData.HasColors)
				{
					channels.Add(new ChannelInfo()
					{
						Dimension = GetColorDimension(version),
						Offset = 0,
						Format = (byte)MeshHelper.GetChannelFormat_Color(version),
						Stream = 3,
					});
					for (int i = 0; i < meshData.Colors.Length; i++)
					{
						writer.Write((Color32)meshData.Colors[i]);
					}
					WriteAlignmentBytes(writer);
				}
				else
				{
					channels.Add(CreateChannelInfoForFloatVector(0, 3, version));
				}

				//kShaderChannelTexCoord0
				if (meshData.HasUV0)
				{
					channels.Add(CreateChannelInfoForFloatVector(2, 4, version));
					Write(writer, meshData.UV0);
					WriteAlignmentBytes(writer);
				}
				else
				{
					channels.Add(CreateChannelInfoForFloatVector(0, 4, version));
				}

				//kShaderChannelTexCoord1
				if (meshData.HasUV1)
				{
					channels.Add(CreateChannelInfoForFloatVector(2, 5, version));
					Write(writer, meshData.UV1);
					WriteAlignmentBytes(writer);
				}
				else
				{
					channels.Add(CreateChannelInfoForFloatVector(0, 5, version));
				}

				//kShaderChannelTexCoord2
				if (meshData.HasUV2)
				{
					channels.Add(CreateChannelInfoForFloatVector(2, 6, version));
					Write(writer, meshData.UV2);
					WriteAlignmentBytes(writer);
				}
				else
				{
					channels.Add(CreateChannelInfoForFloatVector(0, 6, version));
				}

				//kShaderChannelTexCoord3
				if (meshData.HasUV3)
				{
					channels.Add(CreateChannelInfoForFloatVector(2, 7, version));
					Write(writer, meshData.UV3);
					WriteAlignmentBytes(writer);
				}
				else
				{
					channels.Add(CreateChannelInfoForFloatVector(0, 7, version));
				}

				//kShaderChannelTexCoord4
				if (meshData.HasUV4)
				{
					channels.Add(CreateChannelInfoForFloatVector(2, 8, version));
					Write(writer, meshData.UV4);
					WriteAlignmentBytes(writer);
				}
				else
				{
					channels.Add(CreateChannelInfoForFloatVector(0, 8, version));
				}

				//kShaderChannelTexCoord5
				if (meshData.HasUV5)
				{
					channels.Add(CreateChannelInfoForFloatVector(2, 9, version));
					Write(writer, meshData.UV5);
					WriteAlignmentBytes(writer);
				}
				else
				{
					channels.Add(CreateChannelInfoForFloatVector(0, 9, version));
				}

				//kShaderChannelTexCoord6
				if (meshData.HasUV6)
				{
					channels.Add(CreateChannelInfoForFloatVector(2, 10, version));
					Write(writer, meshData.UV6);
					WriteAlignmentBytes(writer);
				}
				else
				{
					channels.Add(CreateChannelInfoForFloatVector(0, 10, version));
				}

				//kShaderChannelTexCoord7
				if (meshData.HasUV7)
				{
					channels.Add(CreateChannelInfoForFloatVector(2, 11, version));
					Write(writer, meshData.UV7);
					WriteAlignmentBytes(writer);
				}
				else
				{
					channels.Add(CreateChannelInfoForFloatVector(0, 11, version));
				}

				if (version.GreaterThanOrEquals(2018, 2))
				{
					if (meshData.HasSkin)
					{
						//todo

						//kShaderChannelBlendWeight
						channels.Add(CreateChannelInfoForFloatVector(0, 12, version));
						//kShaderChannelBlendIndices
						channels.Add(CreateChannelInfoForFloatVector(0, 13, version));
					}
					else
					{
						//kShaderChannelBlendWeight
						channels.Add(CreateChannelInfoForFloatVector(0, 12, version));
						//kShaderChannelBlendIndices
						channels.Add(CreateChannelInfoForFloatVector(0, 13, version));
					}
				}
			}
			else
			{
				//kShaderChannelVertex
				channels.Add(CreateChannelInfoForFloatVector(3, 0, version));
				Write(writer, meshData.Vertices);
				WriteAlignmentBytes(writer);

				//kShaderChannelNormal
				if (meshData.HasNormals)
				{
					// Maybe this should be dimension 4? I've seen models with 4D normals. The W component is always 0.
					channels.Add(CreateChannelInfoForFloatVector(3, 1, version));
					Write(writer, meshData.Normals);
					WriteAlignmentBytes(writer);
				}
				else
				{
					channels.Add(CreateChannelInfoForFloatVector(0, 1, version));
				}

				//kShaderChannelColor
				if (meshData.HasColors)
				{
					channels.Add(new ChannelInfo()
					{
						Dimension = GetColorDimension(version),
						Offset = 0,
						Format = (byte)MeshHelper.GetChannelFormat_Color(version),
						Stream = 2,
					});
					for (int i = 0; i < meshData.Colors.Length; i++)
					{
						writer.Write((Color32)meshData.Colors[i]);
					}
					WriteAlignmentBytes(writer);
				}
				else
				{
					channels.Add(CreateChannelInfoForFloatVector(0, 2, version));
				}

				//kShaderChannelTexCoord0
				if (meshData.HasUV0)
				{
					channels.Add(CreateChannelInfoForFloatVector(2, 3, version));
					Write(writer, meshData.UV0);
					WriteAlignmentBytes(writer);
				}
				else
				{
					channels.Add(CreateChannelInfoForFloatVector(0, 3, version));
				}

				//kShaderChannelTexCoord1
				if (meshData.HasUV1)
				{
					channels.Add(CreateChannelInfoForFloatVector(2, 4, version));
					Write(writer, meshData.UV1);
					WriteAlignmentBytes(writer);
				}
				else
				{
					channels.Add(CreateChannelInfoForFloatVector(0, 4, version));
				}

				if (version.LessThan(5))
				{
					//kShaderChannelTangent
					if (meshData.HasTangents)
					{
						channels.Add(CreateChannelInfoForFloatVector(4, 5, version));
						Write(writer, meshData.Tangents);
						WriteAlignmentBytes(writer);
					}
					else
					{
						channels.Add(CreateChannelInfoForFloatVector(0, 5, version));
					}
				}
				else
				{
					//kShaderChannelTexCoord2
					if (meshData.HasUV2)
					{
						channels.Add(CreateChannelInfoForFloatVector(2, 5, version));
						Write(writer, meshData.UV2);
						WriteAlignmentBytes(writer);
					}
					else
					{
						channels.Add(CreateChannelInfoForFloatVector(0, 5, version));
					}

					//kShaderChannelTexCoord3
					if (meshData.HasUV3)
					{
						channels.Add(CreateChannelInfoForFloatVector(2, 6, version));
						Write(writer, meshData.UV3);
						WriteAlignmentBytes(writer);
					}
					else
					{
						channels.Add(CreateChannelInfoForFloatVector(0, 6, version));
					}

					//kShaderChannelTangent
					if (meshData.HasTangents)
					{
						channels.Add(CreateChannelInfoForFloatVector(4, 7, version));
						Write(writer, meshData.Tangents);
						WriteAlignmentBytes(writer);
					}
					else
					{
						channels.Add(CreateChannelInfoForFloatVector(0, 7, version));
					}
				}
			}

			writer.Flush();
			memoryStream.Flush();
			data = memoryStream.ToArray();
		}

		IReadOnlyList<IStreamInfo> streams = ConvertChannelsToStreams(channels, (uint)meshData.Vertices.Length, version);
		return new VertexDataBlob(channels, streams, data, meshData.Vertices.Length, version, endianType);
	}

	private static ChannelInfo CreateChannelInfoForFloatVector(byte dimension, byte streamIndex, UnityVersion version)
	{
		return new ChannelInfo()
		{
			Dimension = dimension,
			Offset = 0,
			Format = (byte)MeshHelper.ToChannelFormat(MeshHelper.VertexFormat.kVertexFormatFloat, version),
			Stream = streamIndex,
		};
	}

	private static byte GetColorDimension(UnityVersion version)
	{
		// Todo: Need to check various Unity versions
		if (version.GreaterThanOrEquals(2018))
		{
			return 4;
		}
		else
		{
			return 1;
		}
	}

	private static void Write(EndianWriter writer, Vector2[] vertices)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			writer.Write(vertices[i]);
		}
	}

	private static void Write(EndianWriter writer, Vector3[] vertices)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			writer.Write(vertices[i]);
		}
	}

	private static void Write(EndianWriter writer, Vector4[] vertices)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			writer.Write(vertices[i]);
		}
	}

	private static void WriteAlignmentBytes(EndianWriter writer)
	{
		int align = VertexStreamAlign - (int)writer.BaseStream.Position % VertexStreamAlign;
		if (align != VertexStreamAlign)
		{
			Span<byte> span = stackalloc byte[align];
			span.Clear();
			writer.Write(span);
		}
	}

	public MeshData ToMeshData()
	{
		if (ReadData(
			out Vector3[]? vertices,
			out Vector3[]? normals,
			out Vector4[]? tangents,
			out ColorFloat[]? colors,
			out Vector2[]? uv0,
			out Vector2[]? uv1,
			out Vector2[]? uv2,
			out Vector2[]? uv3,
			out Vector2[]? uv4,
			out Vector2[]? uv5,
			out Vector2[]? uv6,
			out Vector2[]? uv7,
			out BoneWeight4[]? skin))
		{
			return new MeshData(vertices, normals, tangents, colors, uv0, uv1, uv2, uv3, uv4, uv5, uv6, uv7, skin, [], [], []);
		}
		else
		{
			return MeshData.Empty;
		}
	}

	public VertexDataBlob MakeLittleEndian()
	{
		if (EndianType == EndianType.LittleEndian)
		{
			return this;
		}

		MeshData meshData = ToMeshData();
		return Create(meshData, Version, EndianType.LittleEndian);
	}

	public bool ReadData(
		[NotNullWhen(true)] out Vector3[]? vertices,
		out Vector3[]? normals,
		out Vector4[]? tangents,
		out ColorFloat[]? colors,
		out Vector2[]? uv0,
		out Vector2[]? uv1,
		out Vector2[]? uv2,
		out Vector2[]? uv3,
		out Vector2[]? uv4,
		out Vector2[]? uv5,
		out Vector2[]? uv6,
		out Vector2[]? uv7,
		out BoneWeight4[]? skin)
	{
		int vertexCount = VertexCount;

		vertices = default;
		normals = default;
		tangents = default;
		colors = default;
		uv0 = default;
		uv1 = default;
		uv2 = default;
		uv3 = default;
		uv4 = default;
		uv5 = default;
		uv6 = default;
		uv7 = default;
		skin = default;

		byte[] data = Data;
		if (data.Length == 0)
		{
			return false;
		}

		for (int chn = 0; chn < Channels.Count; chn++)
		{
			ChannelInfo channel = Channels[chn];

			byte dimension = GetChannelDimension(channel, chn, Version);

			if (dimension == 0)
			{
				continue;
			}
			IStreamInfo m_Stream = Streams[channel.Stream];
			if (!GetBit(m_Stream.ChannelMask, chn))
			{
				continue;
			}

			MeshHelper.VertexFormat vertexFormat = MeshHelper.ToVertexFormat(channel.Format, Version);
			int componentByteSize = MeshHelper.GetFormatSize(vertexFormat);
			byte[] componentBytes = new byte[vertexCount * dimension * componentByteSize];
			for (int v = 0; v < vertexCount; v++)
			{
				int vertexOffset = (int)m_Stream.Offset + channel.Offset + (int)m_Stream.GetStride() * v;
				for (int d = 0; d < dimension; d++)
				{
					int componentOffset = vertexOffset + componentByteSize * d;
					Buffer.BlockCopy(data, componentOffset, componentBytes, componentByteSize * (v * dimension + d), componentByteSize);
				}
			}

			if (EndianType == EndianType.BigEndian && componentByteSize > 1) //swap bytes
			{
				if (componentBytes.Length % componentByteSize != 0)
				{
					throw new InvalidDataException("Component bytes length is not divisible by component byte size");
				}

				switch (componentByteSize)
				{
					case sizeof(ushort):
						for (int i = componentBytes.Length - sizeof(ushort); i >= 0; i -= sizeof(ushort))
						{
							Span<byte> span = componentBytes.AsSpan(i);
							ushort value = BinaryPrimitives.ReadUInt16BigEndian(span);
							BinaryPrimitives.WriteUInt16LittleEndian(span, value);
						}
						break;
					case sizeof(uint):
						for (int i = componentBytes.Length - sizeof(uint); i >= 0; i -= sizeof(uint))
						{
							Span<byte> span = componentBytes.AsSpan(i);
							uint value = BinaryPrimitives.ReadUInt32BigEndian(span);
							BinaryPrimitives.WriteUInt32LittleEndian(span, value);
						}
						break;
				}
			}

			int[] componentsIntArray;
			float[] componentsFloatArray;
			if (MeshHelper.IsIntFormat(vertexFormat))
			{
				componentsIntArray = MeshHelper.BytesToIntArray(componentBytes, vertexFormat);
				componentsFloatArray = [];
			}
			else
			{
				componentsIntArray = [];
				componentsFloatArray = MeshHelper.BytesToFloatArray(componentBytes, vertexFormat);
			}

			if (Version.GreaterThanOrEquals(2018))
			{
				switch (chn)
				{
					case 0: //kShaderChannelVertex
						vertices = MeshHelper.FloatArrayToVector3(componentsFloatArray, dimension);
						break;
					case 1: //kShaderChannelNormal
						normals = MeshHelper.FloatArrayToVector3(componentsFloatArray, dimension);
						break;
					case 2: //kShaderChannelTangent
						tangents = MeshHelper.FloatArrayToVector4(componentsFloatArray, dimension);
						break;
					case 3: //kShaderChannelColor
						colors = MeshHelper.FloatArrayToColorFloat(componentsFloatArray);
						break;
					case 4: //kShaderChannelTexCoord0
						uv0 = MeshHelper.FloatArrayToVector2(componentsFloatArray, dimension);
						break;
					case 5: //kShaderChannelTexCoord1
						uv1 = MeshHelper.FloatArrayToVector2(componentsFloatArray, dimension);
						break;
					case 6: //kShaderChannelTexCoord2
						uv2 = MeshHelper.FloatArrayToVector2(componentsFloatArray, dimension);
						break;
					case 7: //kShaderChannelTexCoord3
						uv3 = MeshHelper.FloatArrayToVector2(componentsFloatArray, dimension);
						break;
					case 8: //kShaderChannelTexCoord4
						uv4 = MeshHelper.FloatArrayToVector2(componentsFloatArray, dimension);
						break;
					case 9: //kShaderChannelTexCoord5
						uv5 = MeshHelper.FloatArrayToVector2(componentsFloatArray, dimension);
						break;
					case 10: //kShaderChannelTexCoord6
						uv6 = MeshHelper.FloatArrayToVector2(componentsFloatArray, dimension);
						break;
					case 11: //kShaderChannelTexCoord7
						uv7 = MeshHelper.FloatArrayToVector2(componentsFloatArray, dimension);
						break;
					//2018.2 and up
					case 12: //kShaderChannelBlendWeight
						skin ??= new BoneWeight4[vertexCount];
						for (int i = 0; i < vertexCount; i++)
						{
							BoneWeight4 boneWeight = skin[i];
							for (int j = 0; j < dimension; j++)
							{
								boneWeight.Weights[j] = componentsFloatArray[i * dimension + j];
							}
							skin[i] = boneWeight;
						}
						break;
					case 13: //kShaderChannelBlendIndices
						skin ??= new BoneWeight4[vertexCount];
						for (int i = 0; i < vertexCount; i++)
						{
							BoneWeight4 boneWeight = skin[i];
							for (int j = 0; j < dimension; j++)
							{
								boneWeight.Indices[j] = componentsIntArray[i * dimension + j];
							}
							skin[i] = boneWeight;
						}
						break;
				}
			}
			else
			{
				switch (chn)
				{
					case 0: //kShaderChannelVertex
						vertices = MeshHelper.FloatArrayToVector3(componentsFloatArray, dimension);
						break;
					case 1: //kShaderChannelNormal
						normals = MeshHelper.FloatArrayToVector3(componentsFloatArray, dimension);
						break;
					case 2: //kShaderChannelColor
						colors = MeshHelper.FloatArrayToColorFloat(componentsFloatArray);
						break;
					case 3: //kShaderChannelTexCoord0
						uv0 = MeshHelper.FloatArrayToVector2(componentsFloatArray, dimension);
						break;
					case 4: //kShaderChannelTexCoord1
						uv1 = MeshHelper.FloatArrayToVector2(componentsFloatArray, dimension);
						break;
					case 5:
						if (Version.GreaterThanOrEquals(5)) //kShaderChannelTexCoord2
						{
							uv2 = MeshHelper.FloatArrayToVector2(componentsFloatArray, dimension);
						}
						else //kShaderChannelTangent
						{
							tangents = MeshHelper.FloatArrayToVector4(componentsFloatArray, dimension);
						}
						break;
					case 6: //kShaderChannelTexCoord3
						uv3 = MeshHelper.FloatArrayToVector2(componentsFloatArray, dimension);
						break;
					case 7: //kShaderChannelTangent
						tangents = MeshHelper.FloatArrayToVector4(componentsFloatArray, dimension);
						break;
				}
			}
		}

		return vertices is not null;
	}

	private static bool GetBit(uint mask, int index)
	{
		return (mask & (1u << index)) != 0;
	}

	public int GetStreamStride(int stream)
	{
		return (int)Streams[stream].GetStride();
	}

	public int GetStreamSize(int stream)
	{
		return GetStreamStride(stream) * VertexCount;
	}

	public int GetStreamOffset(int stream)
	{
		int offset = 0;
		for (int i = 0; i < stream; i++)
		{
			offset += GetStreamSize(i);
			offset = (offset + (VertexStreamAlign - 1)) & ~(VertexStreamAlign - 1);
		}
		return offset;
	}

	private static IStreamInfo[] ConvertChannelsToStreams(IReadOnlyList<ChannelInfo> channels, uint vertexCount, UnityVersion version)
	{
		if (channels.Count == 0)
		{
			return [];
		}

		int streamCount = channels.Max(x => x.Stream) + 1;
		IStreamInfo[] streams = new IStreamInfo[streamCount];
		long offset = 0;
		for (int s = 0; s < streamCount; s++)
		{
			uint chnMask = 0;
			uint stride = 0;
			for (int chn = 0; chn < channels.Count; chn++)
			{
				ChannelInfo channel = channels[chn];
				if (channel.Stream == s)
				{
					byte dimension = GetChannelDimension(channel, chn, version);

					if (dimension > 0)
					{
						chnMask |= 1u << chn;
						stride += dimension * (uint)MeshHelper.GetFormatSize(MeshHelper.ToVertexFormat(channel.Format, version));
					}
				}
			}

			StreamInfo_4 newStream = new()
			{
				ChannelMask = chnMask,
				Offset = (uint)offset,
				Stride_Byte = (byte)stride,
				DividerOp = 0,
				Frequency = 0,
			};
			streams[s] = newStream;

			offset += vertexCount * stride;
			offset = AlignStream(offset);
		}
		return streams;
	}

	private static byte GetChannelDimension(ChannelInfo channel, int chn, UnityVersion version)
	{
		if (version.LessThan(2018) && chn == 2 && channel.Format == 2) //kShaderChannelColor && kChannelFormatColor
		{
			return 4;
		}
		else
		{
			return channel.GetDataDimension();
		}
	}

	private static IReadOnlyList<ChannelInfo> ConvertStreamsToChannels(IReadOnlyList<IStreamInfo> streams, UnityVersion version)
	{
		List<ChannelInfo> channels = new List<ChannelInfo>(6);
		for (int s = 0; s < streams.Count; s++)
		{
			IStreamInfo m_Stream = streams[s];
			byte offset = 0;
			for (int i = 0; i < 6; i++)
			{
				if (!GetBit(m_Stream.ChannelMask, i))
				{
					continue;
				}

				ChannelInfo m_Channel = new();
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
				channels.Add(m_Channel);
			}
		}
		return channels;
	}

	private static long AlignStream(long offset)
	{
		return offset + (VertexStreamAlign - 1L) & ~(VertexStreamAlign - 1L);
	}

	public static bool SupportsUV2(UnityVersion version)
	{
		return version.GreaterThanOrEquals(5);
	}

	public static bool SupportsUV3(UnityVersion version)
	{
		return version.GreaterThanOrEquals(5);
	}

	public static bool SupportsUV4(UnityVersion version)
	{
		return version.GreaterThanOrEquals(2018);
	}

	public static bool SupportsUV5(UnityVersion version)
	{
		return version.GreaterThanOrEquals(2018);
	}

	public static bool SupportsUV6(UnityVersion version)
	{
		return version.GreaterThanOrEquals(2018);
	}

	public static bool SupportsUV7(UnityVersion version)
	{
		return version.GreaterThanOrEquals(2018);
	}

	public static bool SupportsSkin(UnityVersion version)
	{
		return version.GreaterThanOrEquals(2018, 2);
	}
}
