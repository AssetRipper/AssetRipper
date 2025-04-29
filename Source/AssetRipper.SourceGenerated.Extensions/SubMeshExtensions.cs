using AssetRipper.Assets.Collections;
using AssetRipper.IO.Endian;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.ShaderChannel;
using AssetRipper.SourceGenerated.Subclasses.ChannelInfo;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using AssetRipper.SourceGenerated.Subclasses.VertexData;
using System.Buffers.Binary;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class SubMeshExtensions
	{
		/// <summary>
		/// For versions &lt; 4, IsTriStrip is used here instead.<br/>
		/// For it, 0 cooresponds to <see cref="MeshTopology.Triangles"/>,<br/>
		/// and non-zero cooresponds to <see cref="MeshTopology.TriangleStrip"/>.<br/>
		/// This conveniently matches the <see cref="MeshTopology"/> enumeration.
		/// </summary>
		public static MeshTopology GetTopology(this ISubMesh subMesh)
		{
			if (subMesh.Has_Topology())
			{
				return subMesh.TopologyE;
			}
			else
			{
				// https://github.com/AssetRipper/AssetRipper/issues/1759
				return subMesh.IsTriStrip != 0 ? MeshTopology.TriangleStrip : MeshTopology.Triangles;
			}
		}

		public static void SetTopology(this ISubMesh subMesh, MeshTopology topology)
		{
			if (subMesh.Has_Topology())
			{
				subMesh.TopologyE = topology;
			}
			else
			{
				subMesh.IsTriStripE = topology;
			}
		}

		public static uint GetFirstIndex(this ISubMesh subMesh, bool is16BitIndices)
		{
			return is16BitIndices ? subMesh.FirstByte / sizeof(ushort) : subMesh.FirstByte / sizeof(uint);
		}

		public static void SetFirstIndex(this ISubMesh subMesh, bool is16BitIndices, uint firstIndex)
		{
			subMesh.FirstByte = is16BitIndices ? firstIndex * sizeof(ushort) : firstIndex * sizeof(uint);
		}

		private static void UpdateSubMeshVertexRange(UnityVersion version, IMesh mesh, ISubMesh submesh)
		{
			if (submesh.IndexCount == 0)
			{
				submesh.FirstVertex = 0;
				submesh.VertexCount = 0;
				return;
			}

			FindMinMaxIndices(version, mesh, submesh, out int minIndex, out int maxIndex);
			submesh.FirstVertex = (uint)minIndex;
			submesh.VertexCount = (uint)(maxIndex - minIndex + 1);
		}

		private static void FindMinMaxIndices(UnityVersion version, IMesh mesh, ISubMesh submesh, out int min, out int max)
		{
			if (mesh.CompressedMesh.Triangles.IsSet())
			{
				int[] triangles = mesh.CompressedMesh.Triangles.UnpackInts();
				uint offset = mesh.Is16BitIndices()
					? submesh.FirstByte / sizeof(ushort)
					: submesh.FirstByte / sizeof(uint);
				FindMinMaxIndices(triangles, (int)offset, (int)submesh.IndexCount, out min, out max);
				return;
			}
			else if (mesh.Is16BitIndices())
			{
				ReadOnlySpan<byte> indexBuffer = new(mesh.IndexBuffer, (int)submesh.FirstByte, (int)submesh.IndexCount * sizeof(ushort));
				FindMinMax16Indices(indexBuffer, out min, out max);
			}
			else
			{
				ReadOnlySpan<byte> indexBuffer = new(mesh.IndexBuffer, (int)submesh.FirstByte, (int)submesh.IndexCount * sizeof(uint));
				FindMinMax32Indices(indexBuffer, out min, out max);
			}
		}

		private static void FindMinMaxIndices(int[] indexBuffer, int offset, int indexCount, out int min, out int max)
		{
			min = indexBuffer[offset];
			max = indexBuffer[offset];
			int end = offset + indexCount;
			for (int i = offset; i < end; i++)
			{
				int index = indexBuffer[i];
				if (index > max)
				{
					max = index;
				}
				else if (index < min)
				{
					min = index;
				}
			}
		}

		private static void FindMinMax16Indices(ReadOnlySpan<byte> indexBuffer, out int min, out int max)
		{
			min = BinaryPrimitives.ReadUInt16LittleEndian(indexBuffer);//Is this correct on big endian games?
			max = min;
			for (int i = 0; i < indexBuffer.Length; i += sizeof(ushort))
			{
				int index = BinaryPrimitives.ReadUInt16LittleEndian(indexBuffer[i..]);
				if (index > max)
				{
					max = index;
				}
				else if (index < min)
				{
					min = index;
				}
			}
		}

		private static void FindMinMax32Indices(ReadOnlySpan<byte> indexBuffer, out int min, out int max)
		{
			min = BinaryPrimitives.ReadInt32LittleEndian(indexBuffer);//Is this correct on big endian games?
			max = min;
			for (int i = 0; i < indexBuffer.Length; i += sizeof(ushort))
			{
				int index = BinaryPrimitives.ReadInt32LittleEndian(indexBuffer[i..]);
				if (index > max)
				{
					max = index;
				}
				else if (index < min)
				{
					min = index;
				}
			}
		}

		private static void RecalculateSubmeshBounds(IMesh mesh, ISubMesh submesh)
		{
			if (submesh.VertexCount == 0)
			{
				submesh.LocalAABB.Reset();
				return;
			}

			FindMinMaxBounds(mesh, submesh, out Vector3 min, out Vector3 max);
			Vector3 center = (min + max) / 2.0f;
			Vector3 extent = max - center;
			submesh.LocalAABB.CopyValuesFrom(center, extent);
		}

		private static void FindMinMaxBounds(IMesh mesh, ISubMesh submesh, out Vector3 min, out Vector3 max)
		{
			//if (mesh.Has_CompressedMesh())
			{
				if (mesh.CompressedMesh.Vertices.IsSet())
				{
					float[] vertices = mesh.CompressedMesh.Vertices.Unpack();
					FindMinMaxBounds(vertices, (int)submesh.FirstVertex, (int)submesh.VertexCount, out min, out max);
					return;
				}
			}

			FindMinMaxBounds(mesh.Collection, mesh.VertexData, (int)submesh.FirstVertex, (int)submesh.VertexCount, out min, out max);
		}

		private static void FindMinMaxBounds(float[] vertexBuffer, int firstVertex, int vertexCount, out Vector3 min, out Vector3 max)
		{
			int offset = firstVertex * 3;
			min = new Vector3(vertexBuffer[offset], vertexBuffer[offset + 1], vertexBuffer[offset + 2]);
			max = new Vector3(vertexBuffer[offset], vertexBuffer[offset + 1], vertexBuffer[offset + 2]);
			int end = offset + vertexCount * 3;
			for (int i = offset; i < end;)
			{
				float x = vertexBuffer[i++];
				float y = vertexBuffer[i++];
				float z = vertexBuffer[i++];

				if (x > max.X)
				{
					max.X = x;
				}
				else if (x < min.X)
				{
					min.X = x;
				}
				if (y > max.Y)
				{
					max.Y = y;
				}
				else if (y < min.Y)
				{
					min.Y = y;
				}
				if (z > max.Z)
				{
					max.Z = z;
				}
				else if (z < min.Z)
				{
					min.Z = z;
				}
			}
		}

		private static void FindMinMaxBounds(AssetCollection meshCollection, IVertexData vertexData, int firstVertex, int vertexCount, out Vector3 min, out Vector3 max)
		{
			ChannelInfo channel = vertexData.GetChannel(meshCollection.Version, ShaderChannel.Vertex);
			int streamOffset = vertexData.GetStreamOffset(meshCollection.Version, channel.Stream);
			int streamStride = vertexData.GetStreamStride(meshCollection.Version, channel.Stream);
			int extraStride = streamStride - ShaderChannel.Vertex.GetStride(meshCollection.Version);
			int vertexOffset = firstVertex * streamStride;
			int begin = streamOffset + vertexOffset + channel.Offset;

			EndianSpanReader reader = new EndianSpanReader(vertexData.Data, meshCollection.EndianType);
			reader.Position = begin;
			Vector3 dummyVertex = reader.ReadVector3();
			min = dummyVertex;
			max = dummyVertex;

			reader.Position = begin;
			for (int i = 0; i < vertexCount; i++)
			{
				Vector3 vertex = reader.ReadVector3();
				if (vertex.X > max.X)
				{
					max.X = vertex.X;
				}
				else if (vertex.X < min.X)
				{
					min.X = vertex.X;
				}
				if (vertex.Y > max.Y)
				{
					max.Y = vertex.Y;
				}
				else if (vertex.Y < min.Y)
				{
					min.Y = vertex.Y;
				}
				if (vertex.Z > max.Z)
				{
					max.Z = vertex.Z;
				}
				else if (vertex.Z < min.Z)
				{
					min.Z = vertex.Z;
				}
				reader.Position += extraStride;
			}
		}

		private static Vector3 ReadVector3(this ref EndianSpanReader reader)
		{
			return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}
	}
}
