using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files;
using System;
using System.Collections;
using System.Linq;

namespace AssetRipper.Core.Classes.Mesh
{
	public interface IVertexData : IUnityAssetBase
	{
		/// <summary>
		/// Less than 2018.1
		/// </summary>
		uint CurrentChannels { get; set; }
		uint VertexCount { get; set; }
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		IChannelInfo[] Channels { get; }
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		IStreamInfo[] Streams { get; }
		/// <summary>
		/// Actually called m_DataSize
		/// </summary>
		byte[] Data { get; set; }
	}

	public static class VertexDataExtensions
	{
		public static bool IsSet(this IVertexData instance) => instance.VertexCount > 0;

		public static void ReadData(
			this IVertexData instance, 
			UnityVersion version,
			out int vertexCount,
			out Vector3f[] vertices,
			out Vector3f[] normals,
			out Vector4f[] tangents,
			out ColorRGBA32[] colors,
			out BoneWeights4[] skin,
			out Vector2f[] uv0,
			out Vector2f[] uv1,
			out Vector2f[] uv2,
			out Vector2f[] uv3,
			out Vector2f[] uv4,
			out Vector2f[] uv5,
			out Vector2f[] uv6,
			out Vector2f[] uv7)
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

			for (var chn = 0; chn < instance.Channels.Length; chn++)
			{
				var m_Channel = instance.Channels[chn];
				if (m_Channel.GetDataDimension() > 0)
				{
					var m_Stream = instance.Streams[m_Channel.Stream];
					var channelMask = new BitArray(BitConverter.GetBytes(m_Stream.ChannelMask));
					if (channelMask.Get(chn))
					{
						if (version.IsLess(2018) && chn == 2 && m_Channel.Format == 2) //kShaderChannelColor && kChannelFormatColor
						{
							m_Channel.SetDataDimension(4);
						}

						var vertexFormat = MeshHelper.ToVertexFormat(m_Channel.Format, version);
						var componentByteSize = (int)MeshHelper.GetFormatSize(vertexFormat);
						var componentBytes = new byte[vertexCount * m_Channel.GetDataDimension() * componentByteSize];
						for (int v = 0; v < vertexCount; v++)
						{
							var vertexOffset = (int)m_Stream.Offset + m_Channel.Offset + (int)m_Stream.Stride * v;
							for (int d = 0; d < m_Channel.GetDataDimension(); d++)
							{
								var componentOffset = vertexOffset + componentByteSize * d;
								Buffer.BlockCopy(instance.Data, componentOffset, componentBytes, componentByteSize * (v * m_Channel.GetDataDimension() + d), componentByteSize);
							}
						}

						if (instance.EndianType == EndianType.BigEndian && componentByteSize > 1) //swap bytes
						{
							for (var i = 0; i < componentBytes.Length / componentByteSize; i++)
							{
								var buff = new byte[componentByteSize];
								Buffer.BlockCopy(componentBytes, i * componentByteSize, buff, 0, componentByteSize);
								buff = buff.Reverse().ToArray();
								Buffer.BlockCopy(buff, 0, componentBytes, i * componentByteSize, componentByteSize);
							}
						}

						int[] componentsIntArray = null;
						float[] componentsFloatArray = null;
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

		private static T[] MakeInitializedArray<T>(int size) where T : new()
		{
			T[] array = new T[size];
			for(int i = 0; i < size; i++)
			{
				array[i] = new();
			}
			return array;
		}
	}
}
