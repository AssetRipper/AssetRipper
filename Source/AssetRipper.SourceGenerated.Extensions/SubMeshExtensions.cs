using AssetRipper.Assets.Collections;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.ShaderChannel;
using AssetRipper.SourceGenerated.Subclasses.ChannelInfo;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using AssetRipper.SourceGenerated.Subclasses.VertexData;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class SubMeshExtensions
	{
		/// <summary>
		/// For versions &lt; 4, IsTriStrip is used here instead.<br/>
		/// For it, 0 cooresponds to <see cref="MeshTopology.Triangles"/>,<br/>
		/// and 1 cooresponds to <see cref="MeshTopology.TriangleStrip"/>.<br/>
		/// This conveniently matches the <see cref="MeshTopology"/> enumeration.
		/// </summary>
		public static MeshTopology GetTopology(this ISubMesh subMesh)
		{
			return subMesh.Has_Topology() ? subMesh.TopologyE : subMesh.IsTriStripE;
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
				FindMinMax16Indices(mesh.IndexBuffer, (int)submesh.FirstByte, (int)submesh.IndexCount, out min, out max);
			}
			else
			{
				FindMinMax32Indices(mesh.IndexBuffer, (int)submesh.FirstByte, (int)submesh.IndexCount, out min, out max);
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

		private static void FindMinMax16Indices(byte[] indexBuffer, int offset, int indexCount, out int min, out int max)
		{
			min = BitConverter.ToUInt16(indexBuffer, offset);
			max = BitConverter.ToUInt16(indexBuffer, offset);
			int end = offset + indexCount * sizeof(ushort);
			for (int i = offset; i < end; i += sizeof(ushort))
			{
				int index = BitConverter.ToUInt16(indexBuffer, i);
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

		private static void FindMinMax32Indices(byte[] indexBuffer, int offset, int indexCount, out int min, out int max)
		{
			min = BitConverter.ToInt32(indexBuffer, offset);
			max = BitConverter.ToInt32(indexBuffer, offset);
			int end = offset + indexCount * sizeof(int);
			for (int i = offset; i < end; i += sizeof(int))
			{
				int index = BitConverter.ToInt32(indexBuffer, i);
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
			using MemoryStream stream = new MemoryStream(vertexData.Data);
			using AssetReader reader = new AssetReader(stream, meshCollection);
			stream.Position = begin;
			Vector3 dummyVertex = reader.ReadVector3();
			min = dummyVertex;
			max = dummyVertex;

			stream.Position = begin;
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
				stream.Position += extraStride;
			}
		}

		private static Vector3 ReadVector3(this AssetReader reader)
		{
			return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}
	}
}
