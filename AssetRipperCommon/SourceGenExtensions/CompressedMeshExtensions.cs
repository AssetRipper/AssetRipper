using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Math;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.SourceGenerated.Subclasses.CompressedMesh;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class CompressedMeshExtensions
	{
		public static void DecompressCompressedMesh(this ICompressedMesh compressedMesh,
			UnityVersion version,
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
			out Vector2f[]? uv7,
			out Matrix4x4f[]? bindPose,
			out uint[]? processedIndexBuffer)
		{
			int vertexCount = default;
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
			bindPose = default;
			processedIndexBuffer = default;

			//Vertex
			if (compressedMesh.Vertices.NumItems > 0)
			{
				vertexCount = (int)compressedMesh.Vertices.NumItems / 3;
				float[] verticesData = compressedMesh.Vertices.UnpackFloats(3, 3 * 4);
				vertices = MeshHelper.FloatArrayToVector3(verticesData);
			}
			//UV
			if (compressedMesh.UV.NumItems > 0)
			{
				uint m_UVInfo = compressedMesh.UVInfo;
				if (m_UVInfo != 0)
				{
					const int kInfoBitsPerUV = 4;
					const int kUVDimensionMask = 3;
					const int kUVChannelExists = 4;
					const int kMaxTexCoordShaderChannels = 8;

					int uvSrcOffset = 0;
					for (int uvIndex = 0; uvIndex < kMaxTexCoordShaderChannels; uvIndex++)
					{
						uint texCoordBits = m_UVInfo >> (uvIndex * kInfoBitsPerUV);
						texCoordBits &= (1u << kInfoBitsPerUV) - 1u;
						if ((texCoordBits & kUVChannelExists) != 0)
						{
							int uvDim = 1 + (int)(texCoordBits & kUVDimensionMask);
							Vector2f[] m_UV = MeshHelper.FloatArrayToVector2(compressedMesh.UV.UnpackFloats(uvDim, uvDim * 4, uvSrcOffset, vertexCount));
							switch (uvIndex)
							{
								case 0:
									uv0 = m_UV;
									break;
								case 1:
									uv1 = m_UV;
									break;
								case 2:
									uv2 = m_UV;
									break;
								case 3:
									uv3 = m_UV;
									break;
								case 4:
									uv4 = m_UV;
									break;
								case 5:
									uv5 = m_UV;
									break;
								case 6:
									uv6 = m_UV;
									break;
								case 7:
									uv7 = m_UV;
									break;
								default:
									throw new IndexOutOfRangeException();
							}
							uvSrcOffset += uvDim * vertexCount;
						}
					}
				}
				else
				{
					uv0 = MeshHelper.FloatArrayToVector2(compressedMesh.UV.UnpackFloats(2, 2 * 4, 0, vertexCount));
					if (compressedMesh.UV.NumItems >= vertexCount * 4)
					{
						uv1 = MeshHelper.FloatArrayToVector2(compressedMesh.UV.UnpackFloats(2, 2 * 4, vertexCount * 2, vertexCount));
					}
				}
			}
			//BindPose
			if (compressedMesh.Has_BindPoses())
			{
				if (compressedMesh.BindPoses.NumItems > 0)
				{
					bindPose = new Matrix4x4f[compressedMesh.BindPoses.NumItems / 16];
					float[] m_BindPoses_Unpacked = compressedMesh.BindPoses.UnpackFloats(16, 4 * 16);
					float[] buffer = new float[16];
					for (int i = 0; i < bindPose.Length; i++)
					{
						Array.Copy(m_BindPoses_Unpacked, i * 16, buffer, 0, 16);
						bindPose[i] = new Matrix4x4f(buffer);
					}
				}
			}
			//Normal
			if (compressedMesh.Normals.NumItems > 0)
			{
				float[] normalData = compressedMesh.Normals.UnpackFloats(2, 4 * 2);
				int[] signs = compressedMesh.NormalSigns.UnpackInts();
				normals = new Vector3f[compressedMesh.Normals.NumItems / 2];
				for (int i = 0; i < compressedMesh.Normals.NumItems / 2; ++i)
				{
					float x = normalData[i * 2 + 0];
					float y = normalData[i * 2 + 1];
					float zsqr = 1 - x * x - y * y;
					float z;
					if (zsqr >= 0f)
					{
						z = (float)System.Math.Sqrt(zsqr);
					}
					else
					{
						z = 0;
						Vector3f normal = new Vector3f(x, y, z);
						normal.Normalize();
						x = normal.X;
						y = normal.Y;
						z = normal.Z;
					}
					if (signs[i] == 0)
					{
						z = -z;
					}

					normals[i] = new Vector3f(x, y, z);
				}
			}
			//Tangent
			if (compressedMesh.Tangents.NumItems > 0)
			{
				float[] tangentData = compressedMesh.Tangents.UnpackFloats(2, 4 * 2);
				int[] signs = compressedMesh.TangentSigns.UnpackInts();
				tangents = new Vector4f[compressedMesh.Tangents.NumItems / 2];
				for (int i = 0; i < compressedMesh.Tangents.NumItems / 2; ++i)
				{
					float x = tangentData[i * 2 + 0];
					float y = tangentData[i * 2 + 1];
					float zsqr = 1 - x * x - y * y;
					float z;
					if (zsqr >= 0f)
					{
						z = (float)System.Math.Sqrt(zsqr);
					}
					else
					{
						z = 0;
						Vector3f vector3f = new Vector3f(x, y, z);
						vector3f.Normalize();
						x = vector3f.X;
						y = vector3f.Y;
						z = vector3f.Z;
					}
					if (signs[i * 2 + 0] == 0)
					{
						z = -z;
					}

					float w = signs[i * 2 + 1] > 0 ? 1.0f : -1.0f;
					tangents[i] = new Vector4f(x, y, z, w);
				}
			}
			//FloatColor
			if (compressedMesh.Has_FloatColors())
			{
				if (compressedMesh.FloatColors.NumItems > 0)
				{
					colors = MeshHelper.FloatArrayToColorRGBA32(compressedMesh.FloatColors.UnpackFloats(1, 4));
				}
			}
			//Skin
			if (compressedMesh.Weights.NumItems > 0)
			{
				int[] weights = compressedMesh.Weights.UnpackInts();
				int[] boneIndices = compressedMesh.BoneIndices.UnpackInts();

				skin = new BoneWeights4[vertexCount];
				for (int i = 0; i < vertexCount; i++)
				{
					skin[i] = new BoneWeights4();
				}

				int bonePos = 0;
				int boneIndexPos = 0;
				int j = 0;
				int sum = 0;

				for (int i = 0; i < compressedMesh.Weights.NumItems; i++)
				{
					//read bone index and weight.
					skin[bonePos].Weights[j] = weights[i] / 31.0f;
					skin[bonePos].BoneIndices[j] = boneIndices[boneIndexPos++];
					j++;
					sum += weights[i];

					//the weights add up to one. fill the rest for this vertex with zero, and continue with next one.
					if (sum >= 31)
					{
						for (; j < 4; j++)
						{
							skin[bonePos].Weights[j] = 0;
							skin[bonePos].BoneIndices[j] = 0;
						}
						bonePos++;
						j = 0;
						sum = 0;
					}
					//we read three weights, but they don't add up to one. calculate the fourth one, and read
					//missing bone index. continue with next vertex.
					else if (j == 3)
					{
						skin[bonePos].Weights[j] = (31 - sum) / 31.0f;
						skin[bonePos].BoneIndices[j] = boneIndices[boneIndexPos++];
						bonePos++;
						j = 0;
						sum = 0;
					}
				}
			}
			//IndexBuffer
			if (compressedMesh.Triangles.NumItems > 0)
			{
				processedIndexBuffer = Array.ConvertAll(compressedMesh.Triangles.UnpackInts(), x => (uint)x);
			}
			//Color
			if (compressedMesh.Has_Colors() && compressedMesh.Colors.NumItems > 0)
			{
				compressedMesh.Colors.NumItems *= 4;
				compressedMesh.Colors.BitSize /= 4;
				int[] tempColors = compressedMesh.Colors.UnpackInts();
				colors = new ColorRGBA32[compressedMesh.Colors.NumItems / 4];
				for (int v = 0; v < compressedMesh.Colors.NumItems / 4; v++)
				{
					colors[v] = new ColorRGBA32((byte)tempColors[4 * v], (byte)tempColors[4 * v + 1], (byte)tempColors[4 * v + 2], (byte)tempColors[4 * v + 3]);
				}
				compressedMesh.Colors.NumItems /= 4;
				compressedMesh.Colors.BitSize *= 4;
			}
		}

	}
}
