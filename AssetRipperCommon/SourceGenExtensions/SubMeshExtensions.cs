using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Classes.Shader.Enums.ShaderChannel;
using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.IO.Endian;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Subclasses.ChannelInfo;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using AssetRipper.SourceGenerated.Subclasses.Vector3f;
using AssetRipper.SourceGenerated.Subclasses.VertexData;
using System.IO;

namespace AssetRipper.Core.SourceGenExtensions
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
			if (subMesh.Has_Topology())
			{
				return (MeshTopology)subMesh.Topology;
			}
			else
			{
				return (MeshTopology)subMesh.IsTriStrip;
			}
		}

		/*
		/// <summary>
		/// Offset in index buffer
		/// </summary>
		uint FirstByte { get; set; }
		/// <summary>
		/// Offset in Vertices
		/// </summary>
		uint FirstVertex { get; set; }
		 */

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
			bool is16bits = mesh.Is16BitIndices();
			//if (mesh.Has_CompressedMesh_C43())
			{
				if (mesh.CompressedMesh_C43.Triangles.IsSet())
				{
					int[] triangles = mesh.CompressedMesh_C43.Triangles.UnpackInts();
					int firstByte = (int)(is16bits ? submesh.FirstByte * 2 : submesh.FirstByte);
					FindMinMaxIndices(triangles, (int)(firstByte / sizeof(int)), (int)submesh.IndexCount, out min, out max);
					return;
				}
			}

			if (is16bits)
			{
				FindMinMax16Indices(mesh.IndexBuffer_C43, (int)submesh.FirstByte, (int)submesh.IndexCount, out min, out max);
			}
			else
			{
				FindMinMax32Indices(mesh.IndexBuffer_C43, (int)submesh.FirstByte, (int)submesh.IndexCount, out min, out max);
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
			int end = offset + (indexCount * sizeof(ushort));
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
			int end = offset + (indexCount * sizeof(int));
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

		private static void RecalculateSubmeshBounds(LayoutInfo layout, IMesh mesh, ISubMesh submesh)
		{
			if (submesh.VertexCount == 0)
			{
				submesh.LocalAABB.Reset();
				return;
			}

			FindMinMaxBounds(layout, mesh, submesh, out Vector3f min, out Vector3f max);
			Vector3f center = (min + max) / 2.0f;
			Vector3f extent = max - center;
			submesh.LocalAABB.CopyValuesFrom(center, extent);
		}

		private static void FindMinMaxBounds(LayoutInfo layout, IMesh mesh, ISubMesh submesh, out Vector3f min, out Vector3f max)
		{
			//if (mesh.Has_CompressedMesh_C43())
			{
				if (mesh.CompressedMesh_C43.Vertices.IsSet())
				{
					float[] vertices = mesh.CompressedMesh_C43.Vertices.Unpack();
					FindMinMaxBounds(vertices, (int)submesh.FirstVertex, (int)submesh.VertexCount, out min, out max);
					return;
				}
			}

			if (mesh.Has_VertexData_C43())
			{
				FindMinMaxBounds(layout, mesh.VertexData_C43, (int)submesh.FirstVertex, (int)submesh.VertexCount, out min, out max);
			}
			else if (mesh.Has_Vertices_C43())
			{
				FindMinMaxBounds(mesh.Vertices_C43, (int)submesh.FirstVertex, (int)submesh.VertexCount, out min, out max);
			}
			else
			{
				min = Vector3f.Zero;
				max = Vector3f.Zero;
			}
		}

		private static void FindMinMaxBounds(float[] vertexBuffer, int firstVertex, int vertexCount, out Vector3f min, out Vector3f max)
		{
			int offset = firstVertex * 3;
			min = new Vector3f(vertexBuffer[offset], vertexBuffer[offset + 1], vertexBuffer[offset + 2]);
			max = new Vector3f(vertexBuffer[offset], vertexBuffer[offset + 1], vertexBuffer[offset + 2]);
			int end = offset + (vertexCount * 3);
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

		private static void FindMinMaxBounds(AssetList<Vector3f_3_0_0_f5> vertices, int firstVertex, int vertexCount, out Vector3f min, out Vector3f max)
		{
			min = (Vector3f)vertices[firstVertex];
			max = (Vector3f)vertices[firstVertex];
			int end = firstVertex + vertexCount;
			for (int i = firstVertex; i < end; i++)
			{
				Vector3f_3_0_0_f5 vertex = vertices[i];
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
			}
		}

		private static void FindMinMaxBounds(LayoutInfo layout, IVertexData vertexData, int firstVertex, int vertexCount, out Vector3f min, out Vector3f max)
		{
			ChannelInfo channel = vertexData.GetChannel(layout.Version, ShaderChannel.Vertex);
			int streamOffset = vertexData.GetStreamOffset(layout.Version, channel.Stream);
			int streamStride = vertexData.GetStreamStride(layout.Version, channel.Stream);
			int extraStride = streamStride - ShaderChannel.Vertex.GetStride(layout.Version);
			int vertexOffset = firstVertex * streamStride;
			int begin = streamOffset + vertexOffset + channel.Offset;
			using MemoryStream stream = new MemoryStream(vertexData.Data);
			using AssetReader reader = new AssetReader(stream, EndianType.LittleEndian, layout);
			stream.Position = begin;
			Vector3f dummyVertex = reader.ReadAsset<Vector3f>();
			min = dummyVertex;
			max = dummyVertex;

			stream.Position = begin;
			for (int i = 0; i < vertexCount; i++)
			{
				Vector3f vertex = reader.ReadAsset<Vector3f>();
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
	}
}
