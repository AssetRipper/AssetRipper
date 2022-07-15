using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Math;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Subclasses.MeshBlendShape;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class MeshExtensions
	{
		public static bool IsCombinedMesh(this IMesh mesh) => mesh.NameString == "Combined Mesh (root scene)";

		public static void ConvertToEditorFormat(this IMesh mesh)
		{
			mesh.SetMeshOptimizationFlags(MeshOptimizationFlags.Everything);
		}

		public static bool CheckAssetIntegrity(this IMesh mesh)
		{
			if (mesh.Has_StreamData_C43() && mesh.Has_VertexData_C43() && mesh.SerializedFile is not null)
			{
				if (mesh.VertexData_C43.IsSet())
				{
					return mesh.StreamData_C43.CheckIntegrity(mesh.SerializedFile);
				}
			}
			return true;
		}

		public static void ReadData(
			this IMesh mesh,
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
			out uint[] processedIndexBuffer)
		{
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

			if (mesh.Has_VertexData_C43())
			{
				mesh.VertexData_C43?.ReadData(mesh.SerializedFile.Version, mesh.SerializedFile.EndianType,
					out vertices,
					out normals,
					out tangents,
					out colors,
					out skin,
					out uv0,
					out uv1,
					out uv2,
					out uv3,
					out uv4,
					out uv5,
					out uv6,
					out uv7);
			}
			else
			{
				vertices = mesh.Vertices_C43!.Select(v => (Vector3f)v).ToArray();
				normals = mesh.Normals_C43!.Select(n => (Vector3f)n).ToArray();
				tangents = mesh.Tangents_C43!.Select(t => (Vector4f)t).ToArray();
				colors = mesh.Colors_C43!.Select(c => (ColorRGBA32)c).ToArray();
				uv0 = mesh.UV_C43!.Select(v => (Vector2f)v).ToArray();
				uv1 = mesh.UV1_C43!.Select(v => (Vector2f)v).ToArray();
			}

			mesh.CompressedMesh_C43.DecompressCompressedMesh(mesh.SerializedFile.Version,
				out Vector3f[]? compressed_vertices,
				out Vector3f[]? compressed_normals,
				out Vector4f[]? compressed_tangents,
				out ColorRGBA32[]? compressed_colors,
				out BoneWeights4[]? compressed_skin,
				out Vector2f[]? compressed_uv0,
				out Vector2f[]? compressed_uv1,
				out Vector2f[]? compressed_uv2,
				out Vector2f[]? compressed_uv3,
				out Vector2f[]? compressed_uv4,
				out Vector2f[]? compressed_uv5,
				out Vector2f[]? compressed_uv6,
				out Vector2f[]? compressed_uv7,
				out Matrix4x4f[]? compressed_bindPose,
				out uint[]? compressed_processedIndexBuffer);

			vertices ??= compressed_vertices;
			normals ??= compressed_normals;
			tangents ??= compressed_tangents;
			colors ??= compressed_colors;
			skin ??= compressed_skin ?? mesh.Skin_C43?.Select(b => b.ToCommonClass()).ToArray();
			uv0 ??= compressed_uv0;
			uv1 ??= compressed_uv1;
			uv2 ??= compressed_uv2;
			uv3 ??= compressed_uv3;
			uv4 ??= compressed_uv4;
			uv5 ??= compressed_uv5;
			uv6 ??= compressed_uv6;
			uv7 ??= compressed_uv7;
			bindPose = compressed_bindPose;
			processedIndexBuffer = compressed_processedIndexBuffer ?? mesh.GetProcessedIndexBuffer();
		}

		public static byte[] GetChannelsData(this IMesh mesh)
		{
			if (mesh.Has_StreamData_C43() && mesh.StreamData_C43.IsSet())
			{
				return mesh.StreamData_C43.GetContent(mesh.SerializedFile);
			}
			else
			{
				return mesh.VertexData_C43?.Data ?? Array.Empty<byte>();
			}
		}

		public static string? FindBlendShapeNameByCRC(this IMesh mesh, uint crc)
		{
			if (mesh.Has_Shapes_C43())
			{
				return mesh.Shapes_C43.FindShapeNameByCRC(crc);
			}
			else if (mesh.Has_ShapesList_C43())
			{
				foreach (MeshBlendShape_4_1_0_f4 blendShape in mesh.ShapesList_C43)
				{
					if (blendShape.IsCRCMatch(crc))
					{
						return blendShape.Name.String;
					}
				}
			}
			return null;
		}

		public static bool Is16BitIndices(this IMesh mesh)
		{
			if (mesh.Has_Use16BitIndices_C43())
			{
				return mesh.Use16BitIndices_C43 != 0;
			}
			else if (mesh.Has_IndexFormat_C43())
			{
				return mesh.IndexFormat_C43 == (int)IndexFormat.UInt16;
			}
			return true;//Never gets run right now, but really old versions used 16 bit exclusively
		}

		public static MeshOptimizationFlags GetMeshOptimizationFlags(this IMesh mesh)
		{
			if (mesh.Has_MeshOptimizationFlags_C43())
			{
				return (MeshOptimizationFlags)mesh.MeshOptimizationFlags_C43;
			}
			else if (mesh.Has_MeshOptimized_C43())
			{
				return mesh.MeshOptimized_C43 ? MeshOptimizationFlags.Everything : MeshOptimizationFlags.PolygonOrder;
			}
			else
			{
				return default;
			}
		}

		public static void SetMeshOptimizationFlags(this IMesh mesh, MeshOptimizationFlags value)
		{
			if (mesh.Has_MeshOptimizationFlags_C43())
			{
				mesh.MeshOptimizationFlags_C43 = (int)value;
			}
			else if (mesh.Has_MeshOptimized_C43())
			{
				mesh.MeshOptimized_C43 = value == MeshOptimizationFlags.Everything;
			}
		}

		public static MeshCompression GetMeshCompression(this IMesh mesh)
		{
			return (MeshCompression)mesh.MeshCompression_C43;
		}

		public static uint[] GetProcessedIndexBuffer(this IMesh mesh)
		{
			uint[] result;
			if (mesh.Is16BitIndices())
			{
				int indexCount = mesh.IndexBuffer_C43.Length / sizeof(ushort);
				ushort[] rentedBuffer = ArrayPool<ushort>.Shared.Rent(indexCount);
				Buffer.BlockCopy(mesh.IndexBuffer_C43, 0, rentedBuffer, 0, mesh.IndexBuffer_C43.Length);
				result = new uint[indexCount];
				UShortToUInt(rentedBuffer, result, indexCount);
				ArrayPool<ushort>.Shared.Return(rentedBuffer);
			}
			else
			{
				result = new uint[mesh.IndexBuffer_C43.Length / sizeof(uint)];
				Buffer.BlockCopy(mesh.IndexBuffer_C43, 0, result, 0, mesh.IndexBuffer_C43.Length);
			}
			return result;
		}

		public static void SetProcessedIndexBuffer(this IMesh mesh, uint[] indices)
		{
			if (mesh.Is16BitIndices())
			{
				mesh.IndexBuffer_C43 = new byte[indices.Length * sizeof(ushort)];
				ushort[] rentedBuffer = ArrayPool<ushort>.Shared.Rent(indices.Length);
				UIntToUShort(indices, rentedBuffer, indices.Length);
				Buffer.BlockCopy(rentedBuffer, 0, mesh.IndexBuffer_C43, 0, mesh.IndexBuffer_C43.Length);
				ArrayPool<ushort>.Shared.Return(rentedBuffer);
			}
			else
			{
				mesh.IndexBuffer_C43 = new byte[indices.Length * sizeof(uint)];
				Buffer.BlockCopy(indices, 0, mesh.IndexBuffer_C43, 0, mesh.IndexBuffer_C43.Length);
			}
		}

		private static void UShortToUInt(ushort[] sourceArray, uint[] destinationArray, int indexCount)
		{
			if (sourceArray.Length < indexCount || destinationArray.Length < indexCount)
			{
				throw new ArgumentOutOfRangeException(nameof(indexCount));
			}

			for (int i = 0; i < indexCount; i++)
			{
				destinationArray[i] = sourceArray[i];
			}
		}

		private static void UIntToUShort(uint[] sourceArray, ushort[] destinationArray, int indexCount)
		{
			if (sourceArray.Length < indexCount || destinationArray.Length < indexCount)
			{
				throw new ArgumentOutOfRangeException(nameof(indexCount));
			}

			for (int i = 0; i < indexCount; i++)
			{
				destinationArray[i] = (ushort)sourceArray[i];
			}
		}

		private static void ReadTriangles(this IMesh mesh)
		{
			List<uint> indices = new List<uint>();
			List<List<uint>> allTriangles = new List<List<uint>>();
			uint[] processedIndexBuffer = mesh.GetProcessedIndexBuffer();

			foreach (ISubMesh subMesh in mesh.SubMeshes_C43)
			{
				List<uint> submeshTriangles = new List<uint>();
				allTriangles.Add(submeshTriangles);
				uint firstIndex = subMesh.FirstByte / 2;
				if (!mesh.Is16BitIndices())
				{
					firstIndex /= 2;
				}
				uint indexCount = subMesh.IndexCount;
				MeshTopology topology = subMesh.GetTopology();
				if (topology == MeshTopology.Triangles)
				{
					for (int i = 0; i < indexCount; i += 3)
					{
						indices.Add(processedIndexBuffer[firstIndex + i]);
						indices.Add(processedIndexBuffer[firstIndex + i + 1]);
						indices.Add(processedIndexBuffer[firstIndex + i + 2]);
						submeshTriangles.Add(processedIndexBuffer[firstIndex + i]);
						submeshTriangles.Add(processedIndexBuffer[firstIndex + i + 1]);
						submeshTriangles.Add(processedIndexBuffer[firstIndex + i + 2]);
					}
				}
				else if (mesh.SerializedFile.Version.IsLess(4) || topology == MeshTopology.TriangleStrip)
				{
					// de-stripify :
					uint triIndex = 0;
					for (int i = 0; i < indexCount - 2; i++)
					{
						uint a = processedIndexBuffer[firstIndex + i];
						uint b = processedIndexBuffer[firstIndex + i + 1];
						uint c = processedIndexBuffer[firstIndex + i + 2];

						// skip degenerates
						if (a == b || a == c || b == c)
						{
							continue;
						}

						// do the winding flip-flop of strips :
						if ((i & 1) == 1)
						{
							indices.Add(b);
							indices.Add(a);
							submeshTriangles.Add(b);
							submeshTriangles.Add(a);
						}
						else
						{
							indices.Add(a);
							indices.Add(b);
							submeshTriangles.Add(a);
							submeshTriangles.Add(b);
						}
						indices.Add(c);
						submeshTriangles.Add(c);
						triIndex += 3;
					}
					//fix indexCount
					//subMesh.IndexCount = triIndex;
				}
				else if (topology == MeshTopology.Quads)
				{
					for (int q = 0; q < indexCount; q += 4)
					{
						indices.Add(processedIndexBuffer[firstIndex + q]);
						indices.Add(processedIndexBuffer[firstIndex + q + 1]);
						indices.Add(processedIndexBuffer[firstIndex + q + 2]);
						indices.Add(processedIndexBuffer[firstIndex + q]);
						indices.Add(processedIndexBuffer[firstIndex + q + 2]);
						indices.Add(processedIndexBuffer[firstIndex + q + 3]);

						submeshTriangles.Add(processedIndexBuffer[firstIndex + q]);
						submeshTriangles.Add(processedIndexBuffer[firstIndex + q + 1]);
						submeshTriangles.Add(processedIndexBuffer[firstIndex + q + 2]);
						submeshTriangles.Add(processedIndexBuffer[firstIndex + q]);
						submeshTriangles.Add(processedIndexBuffer[firstIndex + q + 2]);
						submeshTriangles.Add(processedIndexBuffer[firstIndex + q + 3]);
					}
					//fix indexCount
					//subMesh.IndexCount = indexCount / 2 * 3;
				}
				else
				{
					//throw new NotSupportedException("Failed getting triangles. Submesh topology is lines or points.");
				}
			}
		}
	}
}
