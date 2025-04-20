using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.ShaderChannel;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.VertexFormat;
using AssetRipper.SourceGenerated.Subclasses.ChannelInfo;
using AssetRipper.SourceGenerated.Subclasses.StreamInfo;
using AssetRipper.SourceGenerated.Subclasses.StreamingInfo;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using AssetRipper.SourceGenerated.Subclasses.VertexData;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions;

public static class VertexDataExtensions
{
	private const int VertexStreamAlign = 16;

	public static bool IsSet(this IVertexData instance, IStreamingInfo? streamingInfo)
	{
		return instance.VertexCount > 0 && (instance.Data.Length > 0 || streamingInfo is not null && streamingInfo.IsSet());
	}

	public static IReadOnlyList<IStreamInfo>? GetStreams(this IVertexData instance)
	{
		if (instance.Has_Streams())
		{
			return instance.Streams;
		}
		else if (instance.Has_Streams_0_())
		{
			return
			[
				instance.Streams_0_,
				instance.Streams_1_,
				instance.Streams_2_,
				instance.Streams_3_
			];
		}
		else
		{
			return null;
		}
	}

	public static uint GetCurrentChannels(this IVertexData instance)
	{
		if (instance.Has_CurrentChannels_Int32())
		{
			return unchecked((uint)instance.CurrentChannels_Int32);
		}
		else
		{
			return instance.CurrentChannels_UInt32;
		}
	}

	public static void SetCurrentChannels(this IVertexData instance, uint value)
	{
		if (instance.Has_CurrentChannels_Int32())
		{
			instance.CurrentChannels_Int32 = unchecked((int)value);
		}
		else
		{
			instance.CurrentChannels_UInt32 = value;
		}
	}

	/// <summary>
	/// 5.6.0
	/// </summary>
	private static bool AllowUnsetVertexChannel(UnityVersion version) => version.Equals(5, 6, 0);

	public static ChannelInfo GetChannel(this IVertexData instance, UnityVersion version, ShaderChannel channelType)
	{
		if (instance.Has_Channels())
		{
			return instance.Channels[channelType.ToChannel(version)];
		}
		else
		{
			IReadOnlyList<IStreamInfo> streams = instance.GetStreams()!;
			ChannelInfo channelInfo = new();
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

	public static BoneWeight4[] GenerateSkin(this IVertexData instance, UnityVersion version)
	{
		if (instance.Channels is null)
		{
			throw new NotImplementedException("GenerateSkin is not implemented for this version.");
		}
		ChannelInfo weightChannel = instance.Channels[(int)ShaderChannel2018.SkinWeight];
		ChannelInfo indexChannel = instance.Channels[(int)ShaderChannel2018.SkinBoneIndex];
		if (!weightChannel.IsSet())
		{
			return Array.Empty<BoneWeight4>();
		}

		BoneWeight4[] skin = new BoneWeight4[instance.VertexCount];
		int weightStride = instance.Channels.Where(t => t.Stream == weightChannel.Stream).Sum(t => t.GetStride(version));
		int weightStreamOffset = instance.GetStreamOffset(version, weightChannel.Stream);
		int indexStride = instance.Channels.Where(t => t.Stream == indexChannel.Stream).Sum(t => t.GetStride(version));
		int indexStreamOffset = instance.GetStreamOffset(version, indexChannel.Stream);

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
		int streamStride = instance.GetStreamStride(version, channel.Stream);
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
		return instance.HasStreamsInvariant() ?
			(int)instance.GetStreamsInvariant()[stream].GetStride() : instance.Channels!.Where(t => t.IsSet() && t.Stream == stream).Sum(t => t.GetStride(version));
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

	private static bool HasStreamsInvariant(this IVertexData instance) => instance.Has_Streams() || instance.Has_Streams_0_();

	private static IReadOnlyList<IStreamInfo> GetStreamsInvariant(this IVertexData instance)
	{
		if (instance.Has_Streams())
		{
			return instance.Streams;
		}
		else if (instance.Has_Streams_0_())
		{
			return new IStreamInfo[]
			{
				instance.Streams_0_,
				instance.Streams_1_,
				instance.Streams_2_,
				instance.Streams_3_
			};
		}
		else
		{
			return Array.Empty<IStreamInfo>();
		}
	}
}
