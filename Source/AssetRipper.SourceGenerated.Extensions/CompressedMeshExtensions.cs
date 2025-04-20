﻿using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Subclasses.CompressedMesh;
using AssetRipper.SourceGenerated.Subclasses.PackedBitVector_Single;
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class CompressedMeshExtensions
	{
		public static bool IsSet(this ICompressedMesh compressedMesh) => compressedMesh.Vertices.NumItems > 0;

		public static void DecompressCompressedMesh(this ICompressedMesh compressedMesh,
			UnityVersion version,
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
			out Vector2[]? uv7,
			out Matrix4x4[]? bindPose,
			out uint[]? processedIndexBuffer)
		{
			vertices = default;
			normals = default;
			tangents = default;
			colors = default;
			skin = default;
			bindPose = default;
			processedIndexBuffer = default;

			//Vertex
			if (compressedMesh.Vertices.NumItems > 0)
			{
				vertices = compressedMesh.GetVertices();
			}
			//UV
			compressedMesh.GetUV(out uv0, out uv1, out uv2, out uv3, out uv4, out uv5, out uv6, out uv7);
			//BindPose
			if (compressedMesh.Has_BindPoses() && compressedMesh.BindPoses.NumItems > 0)
			{
				bindPose = compressedMesh.GetBindPoses();
			}
			//Normal
			if (compressedMesh.Normals.NumItems > 0)
			{
				normals = compressedMesh.GetNormals();
			}
			//Tangent
			if (compressedMesh.Tangents.NumItems > 0)
			{
				tangents = compressedMesh.GetTangents();
			}
			//FloatColor / Color
			if (compressedMesh.Has_FloatColors() && compressedMesh.FloatColors.NumItems > 0
				|| compressedMesh.Has_Colors() && compressedMesh.Colors.NumItems > 0)
			{
				colors = compressedMesh.GetFloatColors();
			}
			//Skin
			if (compressedMesh.Weights.NumItems > 0)
			{
				skin = compressedMesh.GetWeights();
			}
			//IndexBuffer
			if (compressedMesh.Triangles.NumItems > 0)
			{
				processedIndexBuffer = compressedMesh.GetTriangles();
			}
		}

		private static int GetVertexCount(ICompressedMesh compressedMesh)
		{
			return (int)compressedMesh.Vertices.NumItems / 3;//3 floats in a Vector3
		}

		public static void GetUV(this ICompressedMesh compressedMesh, out Vector2[]? uv0, out Vector2[]? uv1, out Vector2[]? uv2, out Vector2[]? uv3, out Vector2[]? uv4, out Vector2[]? uv5, out Vector2[]? uv6, out Vector2[]? uv7)
		{
			int vertexCount = GetVertexCount(compressedMesh);
			if (compressedMesh.UV.NumItems > 0)
			{
				UVInfo m_UVInfo = compressedMesh.UVInfo;
				if (compressedMesh.Has_UVInfo() && m_UVInfo != UVInfo.Zero)
				{
					int uvSrcOffset = 0;
					uv0 = ReadChannel(compressedMesh.UV, m_UVInfo, 0, vertexCount, ref uvSrcOffset);
					uv1 = ReadChannel(compressedMesh.UV, m_UVInfo, 1, vertexCount, ref uvSrcOffset);
					uv2 = ReadChannel(compressedMesh.UV, m_UVInfo, 2, vertexCount, ref uvSrcOffset);
					uv3 = ReadChannel(compressedMesh.UV, m_UVInfo, 3, vertexCount, ref uvSrcOffset);
					uv4 = ReadChannel(compressedMesh.UV, m_UVInfo, 4, vertexCount, ref uvSrcOffset);
					uv5 = ReadChannel(compressedMesh.UV, m_UVInfo, 5, vertexCount, ref uvSrcOffset);
					uv6 = ReadChannel(compressedMesh.UV, m_UVInfo, 6, vertexCount, ref uvSrcOffset);
					uv7 = ReadChannel(compressedMesh.UV, m_UVInfo, 7, vertexCount, ref uvSrcOffset);
				}
				else
				{
					uv0 = MeshHelper.FloatArrayToVector2(compressedMesh.UV.UnpackFloats(2, 2 * sizeof(float), 0, vertexCount));
					if (compressedMesh.UV.NumItems >= vertexCount * sizeof(float))
					{
						uv1 = MeshHelper.FloatArrayToVector2(compressedMesh.UV.UnpackFloats(2, 2 * sizeof(float), vertexCount * 2, vertexCount));
					}
					else
					{
						uv1 = default;
					}
					uv2 = default;
					uv3 = default;
					uv4 = default;
					uv5 = default;
					uv6 = default;
					uv7 = default;
				}
			}
			else
			{
				uv0 = default;
				uv1 = default;
				uv2 = default;
				uv3 = default;
				uv4 = default;
				uv5 = default;
				uv6 = default;
				uv7 = default;
			}
		}

		private static Vector2[]? ReadChannel(PackedBitVector_Single packedVector, UVInfo uvInfo, int channelIndex, int vertexCount, ref int currentOffset)
		{
			uvInfo.GetChannelInfo(channelIndex, out bool exists, out int uvDim);
			if (exists)
			{
				Vector2[] m_UV = MeshHelper.FloatArrayToVector2(packedVector.UnpackFloats(uvDim, uvDim * sizeof(float), currentOffset, vertexCount));
				currentOffset += uvDim * vertexCount;
				return m_UV;
			}
			else
			{
				return null;
			}
		}

		public static void SetUV(this ICompressedMesh compressedMesh, ReadOnlySpan<Vector2> uv0, ReadOnlySpan<Vector2> uv1, ReadOnlySpan<Vector2> uv2, ReadOnlySpan<Vector2> uv3, ReadOnlySpan<Vector2> uv4, ReadOnlySpan<Vector2> uv5, ReadOnlySpan<Vector2> uv6, ReadOnlySpan<Vector2> uv7)
		{
			if (!compressedMesh.Has_UVInfo() || AllEmpty(uv2, uv3, uv4, uv5, uv6, uv7))
			{
				compressedMesh.UVInfo = 0;
				if (uv0.Length == 0)
				{
					compressedMesh.UV.PackFloats(default);
				}
				else if (uv1.Length == 0)
				{
					compressedMesh.UV.Pack(uv0);
				}
				else if (uv0.Length != uv1.Length)
				{
					throw new ArgumentException("UV arrays must be the same length.");
				}
				else
				{
					int length = uv0.Length + uv1.Length;
					using RentedArray<Vector2> concatenated = new(length);
					uv0.CopyTo(concatenated);
					uv1.CopyTo(concatenated.Slice(uv0.Length));
					compressedMesh.UV.Pack<Vector2>(concatenated);
				}
			}
			else
			{
				int totalLength = uv0.Length + uv1.Length + uv2.Length + uv3.Length + uv4.Length + uv5.Length + uv6.Length + uv7.Length;
				using RentedArray<Vector2> buffer = new(totalLength);
				int currentOffset = 0;
				UVInfo uvInfo = default;
				UpdateBuffer(uv0, 0, buffer, ref currentOffset, ref uvInfo);
				UpdateBuffer(uv1, 1, buffer, ref currentOffset, ref uvInfo);
				UpdateBuffer(uv2, 2, buffer, ref currentOffset, ref uvInfo);
				UpdateBuffer(uv3, 3, buffer, ref currentOffset, ref uvInfo);
				UpdateBuffer(uv4, 4, buffer, ref currentOffset, ref uvInfo);
				UpdateBuffer(uv5, 5, buffer, ref currentOffset, ref uvInfo);
				UpdateBuffer(uv6, 6, buffer, ref currentOffset, ref uvInfo);
				UpdateBuffer(uv7, 7, buffer, ref currentOffset, ref uvInfo);
				compressedMesh.UV.Pack<Vector2>(buffer);
				compressedMesh.UVInfo = uvInfo;
			}

			static bool AllEmpty(ReadOnlySpan<Vector2> uv2, ReadOnlySpan<Vector2> uv3, ReadOnlySpan<Vector2> uv4, ReadOnlySpan<Vector2> uv5, ReadOnlySpan<Vector2> uv6, ReadOnlySpan<Vector2> uv7)
			{
				return uv2.Length == 0 && uv3.Length == 0 && uv4.Length == 0 && uv5.Length == 0 && uv6.Length == 0 && uv7.Length == 0;
			}

			static void UpdateBuffer(ReadOnlySpan<Vector2> uv, int uvIndex, Span<Vector2> buffer, ref int currentOffset, ref UVInfo uvInfo)
			{
				if (uv.Length > 0)
				{
					uvInfo = uvInfo.AddChannelInfo(uvIndex, true, 2);
					uv.CopyTo(buffer[currentOffset..]);
					currentOffset += uv.Length;
				}
			}
		}

		public static BoneWeight4[] GetWeights(this ICompressedMesh compressedMesh)
		{
			int[] weights = compressedMesh.Weights.UnpackInts();
			int[] boneIndices = compressedMesh.BoneIndices.UnpackInts();

			//In theory, the array length should be exactly the same as the number of vertices, but it's better to be safe.
			BoneWeight4[] skin = ArrayPool<BoneWeight4>.Shared.Rent((int)compressedMesh.Weights.NumItems);

			int bonePos = 0;
			int boneIndexPos = 0;
			int j = 0;
			int sum = 0;

			for (int i = 0; i < compressedMesh.Weights.NumItems; i++)
			{
				//read bone index and weight.
				{
					BoneWeight4 boneWeight = skin[bonePos];
					boneWeight.Weights[j] = weights[i] / 31f;
					boneWeight.Indices[j] = boneIndices[boneIndexPos++];
					skin[bonePos] = boneWeight;
				}
				j++;
				sum += weights[i];

				//the weights add up to one. fill the rest for this vertex with zero, and continue with next one.
				if (sum >= 31)
				{
					for (; j < 4; j++)
					{
						BoneWeight4 boneWeight = skin[bonePos];
						boneWeight.Weights[j] = 0;
						boneWeight.Indices[j] = 0;
						skin[bonePos] = boneWeight;
					}
					bonePos++;
					j = 0;
					sum = 0;
				}
				//we read three weights, but they don't add up to one. calculate the fourth one, and read
				//missing bone index. continue with next vertex.
				else if (j == 3)
				{
					BoneWeight4 boneWeight = skin[bonePos];
					boneWeight.Weights[j] = (31 - sum) / 31f;
					boneWeight.Indices[j] = boneIndices[boneIndexPos++];
					skin[bonePos] = boneWeight;
					bonePos++;
					j = 0;
					sum = 0;
				}
			}

			BoneWeight4[] result = skin.AsSpan(0, bonePos).ToArray();
			ArrayPool<BoneWeight4>.Shared.Return(skin);
			return result;
		}

		public static void SetWeights(this ICompressedMesh compressedMesh, ReadOnlySpan<BoneWeight4> skin)
		{
			if (skin.Length > 0)
			{
				int i_weight = 0;
				int i_boneIndex = 0;
				int[] weightList = ArrayPool<int>.Shared.Rent(skin.Length * 3);
				int[] boneIndexList = ArrayPool<int>.Shared.Rent(skin.Length * 4);

				foreach (BoneWeight4 boneWeight in skin)
				{
					int sum = 0;
					for (int j = 0; j < 4; j++)
					{
						int weight = (int)(boneWeight.Weights[j] * 31);
						sum += weight;
						if (j != 3)
						{
							//We never store the last weight because it can be calculated from the sum of the other weights.
							weightList[i_weight] = weight;
							i_weight++;
						}

						boneIndexList[i_boneIndex] = boneWeight.Indices[j];
						i_boneIndex++;

						if (sum >= 31)
						{
							break;
						}
					}
				}

				compressedMesh.Weights.PackInts(weightList.AsSpan(0, i_weight));
				compressedMesh.BoneIndices.PackInts(boneIndexList.AsSpan(0, i_boneIndex));

				ArrayPool<int>.Shared.Return(weightList);
				ArrayPool<int>.Shared.Return(boneIndexList);
			}
			else
			{
				compressedMesh.Weights.Reset();
				compressedMesh.BoneIndices.Reset();
			}
		}

		public static Vector3[] GetNormals(this ICompressedMesh compressedMesh)
		{
			float[] normalData = compressedMesh.Normals.UnpackFloats(2, 2 * sizeof(float));
			int[] signs = compressedMesh.NormalSigns.UnpackInts();
			Vector3[] normals = new Vector3[compressedMesh.Normals.NumItems / 2];
			for (int i = 0; i < compressedMesh.Normals.NumItems / 2; ++i)
			{
				float x = normalData[i * 2 + 0];
				float y = normalData[i * 2 + 1];
				float zsqr = 1 - x * x - y * y;
				float z;
				if (zsqr >= 0)
				{
					z = (float)Math.Sqrt(zsqr);
				}
				else
				{
					z = 0;
					Vector3 normal = Vector3.Normalize(new Vector3(x, y, z));
					x = normal.X;
					y = normal.Y;
					z = normal.Z;
				}
				if (signs[i] == 0)
				{
					z = -z;
				}

				normals[i] = new Vector3(x, y, z);
			}

			return normals;
		}

		public static void SetNormals(this ICompressedMesh compressedMesh, ReadOnlySpan<Vector3> normals)
		{
			MakeFloatAndSignArrays(normals, out float[] floats, out uint[] signs);
			compressedMesh.Normals.PackFloats(floats);
			compressedMesh.NormalSigns.PackUInts(signs);
		}

		private static void MakeFloatAndSignArrays(ReadOnlySpan<Vector3> normals, out float[] floats, out uint[] signs)
		{
			floats = new float[normals.Length * 2];
			signs = new uint[normals.Length];
			for (int i = 0; i < normals.Length; i++)
			{
				//Normals should already be normalized, but it's better to be safe.
				Vector3 vector = Vector3.Normalize(normals[i]);
				floats[2 * i] = vector.X;
				floats[2 * i + 1] = vector.Y;
				signs[i] = vector.Z < 0 ? 0u : 1u;
			}
		}

		public static Vector4[] GetTangents(this ICompressedMesh compressedMesh)
		{
			float[] tangentData = compressedMesh.Tangents.UnpackFloats(2, 2 * sizeof(float));
			int[] signs = compressedMesh.TangentSigns.UnpackInts();
			Vector4[] tangents = new Vector4[compressedMesh.Tangents.NumItems / 2];
			for (int i = 0; i < compressedMesh.Tangents.NumItems / 2; ++i)
			{
				float x = tangentData[i * 2 + 0];
				float y = tangentData[i * 2 + 1];
				float zsqr = 1 - x * x - y * y;
				float z;
				if (zsqr >= 0f)
				{
					z = (float)Math.Sqrt(zsqr);
				}
				else
				{
					z = 0;
					Vector3 tangent = Vector3.Normalize(new Vector3(x, y, z));
					x = tangent.X;
					y = tangent.Y;
					z = tangent.Z;
				}
				if (signs[i * 2 + 0] == 0)
				{
					z = -z;
				}

				float w = signs[i * 2 + 1] == 0 ? -1.0f : 1.0f;
				tangents[i] = new Vector4(x, y, z, w);
			}

			return tangents;
		}

		public static void SetTangents(this ICompressedMesh compressedMesh, ReadOnlySpan<Vector4> tangents)
		{
			MakeFloatAndSignArrays(tangents, out float[] floats, out uint[] signs);
			compressedMesh.Tangents.PackFloats(floats);
			compressedMesh.TangentSigns.PackUInts(signs);
		}

		private static void MakeFloatAndSignArrays(ReadOnlySpan<Vector4> tangents, out float[] floats, out uint[] signs)
		{
			floats = new float[tangents.Length * 2];
			signs = new uint[tangents.Length * 2];
			for (int i = 0; i < tangents.Length; i++)
			{
				//Tangents should already be normalized, but it's better to be safe.
				Vector3 vector = Vector3.Normalize(tangents[i].AsVector3());
				floats[2 * i] = vector.X;
				floats[2 * i + 1] = vector.Y;
				signs[2 * i] = vector.Z < 0 ? 0u : 1u;
				signs[2 * i + 1] = tangents[i].W < 0 ? 0u : 1u;
			}
		}

		/// <summary>
		/// Only available before Unity 5
		/// </summary>
		/// <param name="compressedMesh"></param>
		/// <returns></returns>
		public static Matrix4x4[] GetBindPoses(this ICompressedMesh compressedMesh)
		{
			if (compressedMesh.Has_BindPoses())
			{
				const int MatrixFloats = 16;
				Matrix4x4[] bindPose = new Matrix4x4[compressedMesh.BindPoses.NumItems / MatrixFloats];
				float[] m_BindPoses_Unpacked = compressedMesh.BindPoses.UnpackFloats(MatrixFloats, MatrixFloats * sizeof(float));
				MemoryMarshal.Cast<float, Matrix4x4>(m_BindPoses_Unpacked).CopyTo(bindPose);
				return bindPose;
			}
			else
			{
				return Array.Empty<Matrix4x4>();
			}
		}

		/// <summary>
		/// Only available before Unity 5
		/// </summary>
		/// <param name="compressedMesh"></param>
		/// <param name="bindPoses"></param>
		public static void SetBindPoses(this ICompressedMesh compressedMesh, ReadOnlySpan<Matrix4x4> bindPoses)
		{
			compressedMesh.BindPoses?.PackFloats(MemoryMarshal.Cast<Matrix4x4, float>(bindPoses));
		}

		public static Vector3[] GetVertices(this ICompressedMesh compressedMesh)
		{
			float[] verticesData = compressedMesh.Vertices.UnpackFloats(3, 3 * sizeof(float));
			return MeshHelper.FloatArrayToVector3(verticesData);
		}

		public static void SetVertices(this ICompressedMesh compressedMesh, ReadOnlySpan<Vector3> vertices)
		{
			compressedMesh.Vertices.PackFloats(MemoryMarshal.Cast<Vector3, float>(vertices));
		}

		public static ColorFloat[] GetFloatColors(this ICompressedMesh compressedMesh)
		{
			if (compressedMesh.Has_FloatColors())
			{
				return MeshHelper.FloatArrayToColorFloat(compressedMesh.FloatColors.UnpackFloats(1, 4));
			}
			else if (compressedMesh.Has_Colors())
			{
				compressedMesh.Colors.NumItems *= 4;
				compressedMesh.Colors.BitSize /= 4;
				int[] tempColors = compressedMesh.Colors.UnpackInts();
				ColorFloat[] colors = new ColorFloat[compressedMesh.Colors.NumItems / 4];
				for (int v = 0; v < compressedMesh.Colors.NumItems / 4; v++)
				{
					colors[v] = (ColorFloat)new Color32((byte)tempColors[4 * v], (byte)tempColors[4 * v + 1], (byte)tempColors[4 * v + 2], (byte)tempColors[4 * v + 3]);
				}
				compressedMesh.Colors.NumItems /= 4;
				compressedMesh.Colors.BitSize *= 4;
				return colors;
			}
			else
			{
				return Array.Empty<ColorFloat>();
			}
		}

		public static void SetFloatColors(this ICompressedMesh compressedMesh, ReadOnlySpan<ColorFloat> colors)
		{
			if (compressedMesh.Has_FloatColors())
			{
				compressedMesh.FloatColors.Pack(colors);
			}
			else if (compressedMesh.Has_Colors())
			{
				Color32[] buffer = ArrayPool<Color32>.Shared.Rent(colors.Length);
				for (int i = 0; i < colors.Length; i++)
				{
					buffer[i] = (Color32)colors[i];
				}
				compressedMesh.Colors.PackUInts(MemoryMarshal.Cast<Color32, uint>(new ReadOnlySpan<Color32>(buffer, 0, colors.Length)));
				ArrayPool<Color32>.Shared.Return(buffer);
			}
		}

		public static uint[] GetTriangles(this ICompressedMesh compressedMesh)
		{
			return compressedMesh.Triangles.UnpackUInts();
		}

		public static void SetTriangles(this ICompressedMesh compressedMesh, ReadOnlySpan<uint> triangles)
		{
			compressedMesh.Triangles.PackUInts(triangles);
		}

		private readonly ref struct RentedArray<T>
		{
			private readonly int length;
			private readonly T[]? array;

			public RentedArray(int length)
			{
				this.length = length;
				array = ArrayPool<T>.Shared.Rent(length);
			}

			public void Dispose()
			{
				if (array != null)
				{
					ArrayPool<T>.Shared.Return(array);
				}
			}

			public Span<T> AsSpan() => new Span<T>(array, 0, length);

			public Span<T> Slice(int start) => new Span<T>(array, start, length - start);

			//Implicit conversions to Span<T> and ReadOnlySpan<T>
			public static implicit operator Span<T>(RentedArray<T> rentedArray) => rentedArray.AsSpan();
			public static implicit operator ReadOnlySpan<T>(RentedArray<T> rentedArray) => rentedArray.AsSpan();
		}
	}
}
