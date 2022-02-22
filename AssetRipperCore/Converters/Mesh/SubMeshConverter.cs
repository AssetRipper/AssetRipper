using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Classes.Misc.Serializable.Boundaries;
using AssetRipper.Core.Classes.Shader.Enums.ShaderChannel;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Math.PackedBitVectors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using System;
using System.IO;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Core.Converters.Mesh
{
	public static class SubMeshConverter
	{
		public static void CalculateSubMeshVertexRangeAndBounds(LayoutInfo layout, AssetRipper.Core.Classes.Mesh.Mesh mesh, SubMesh submesh)
		{
			UpdateSubMeshVertexRange(layout.Version, mesh, submesh);
			RecalculateSubmeshBounds(layout, mesh, submesh);
		}

		public static SubMesh[] Convert(IExportContainer container, AssetRipper.Core.Classes.Mesh.Mesh instanceMesh, SubMesh[] origin)
		{
			SubMesh[] instances = new SubMesh[origin.Length];
			for (int i = 0; i < origin.Length; i++)
			{
				instances[i] = Convert(container, instanceMesh, origin[i]);
			}
			return instances;
		}

		private static SubMesh Convert(IExportContainer container, AssetRipper.Core.Classes.Mesh.Mesh instanceMesh, SubMesh origin)
		{
			SubMesh instance = new SubMesh();
			instance.FirstByte = origin.FirstByte;
			instance.IndexCount = origin.IndexCount;
			instance.Topology = origin.GetTopology(container.Version);
			if (SubMesh.HasTriangleCount(container.ExportVersion))
			{
				instance.TriangleCount = origin.TriangleCount;
			}
			if (SubMesh.HasBaseVertex(container.ExportVersion))
			{
				instance.BaseVertex = (uint)GetBaseVertex(container, origin);
			}
			if (SubMesh.HasVertex(container.ExportVersion))
			{
				SetVertex(container, instanceMesh, origin, instance);
			}
			return instance;
		}

		private static int GetBaseVertex(IExportContainer container, SubMesh origin)
		{
			if (SubMesh.HasBaseVertex(container.Version))
			{
				return (int)origin.BaseVertex;
			}
#warning TODO: calculate or default value?
			return 0;
		}

		private static void SetVertex(IExportContainer container, AssetRipper.Core.Classes.Mesh.Mesh instanceMesh, SubMesh origin, SubMesh instance)
		{
			if (SubMesh.HasVertex(container.Version))
			{
				instance.FirstVertex = origin.FirstVertex;
				instance.VertexCount = origin.VertexCount;
				instance.LocalAABB.CopyValuesFrom(origin.LocalAABB);
			}
			else
			{
				CalculateSubMeshVertexRangeAndBounds(container.ExportLayout, instanceMesh, instance);
			}
		}

		private static void UpdateSubMeshVertexRange(UnityVersion version, AssetRipper.Core.Classes.Mesh.Mesh mesh, SubMesh submesh)
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

		private static void FindMinMaxIndices(UnityVersion version, AssetRipper.Core.Classes.Mesh.Mesh mesh, SubMesh submesh, out int min, out int max)
		{
			bool is16bits = mesh.Is16BitIndices(version);
			if (AssetRipper.Core.Classes.Mesh.Mesh.HasCompressedMesh(version))
			{
				if (mesh.CompressedMesh.Triangles.IsSet())
				{
					int[] triangles = mesh.CompressedMesh.Triangles.UnpackInts();
					int firstByte = (int)(is16bits ? submesh.FirstByte * 2 : submesh.FirstByte);
					FindMinMaxIndices(triangles, (int)(firstByte / sizeof(int)), (int)submesh.IndexCount, out min, out max);
					return;
				}
			}

			if (is16bits)
			{
				FindMinMax16Indices(mesh.RawIndexBuffer, (int)submesh.FirstByte, (int)submesh.IndexCount, out min, out max);
			}
			else
			{
				FindMinMax32Indices(mesh.RawIndexBuffer, (int)submesh.FirstByte, (int)submesh.IndexCount, out min, out max);
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

		private static void RecalculateSubmeshBounds(LayoutInfo layout, AssetRipper.Core.Classes.Mesh.Mesh mesh, SubMesh submesh)
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

		private static void FindMinMaxBounds(LayoutInfo layout, AssetRipper.Core.Classes.Mesh.Mesh mesh, SubMesh submesh, out Vector3f min, out Vector3f max)
		{
			if (AssetRipper.Core.Classes.Mesh.Mesh.HasCompressedMesh(layout.Version))
			{
				if (mesh.CompressedMesh.Vertices.IsSet())
				{
					float[] vertices = mesh.CompressedMesh.Vertices.Unpack();
					FindMinMaxBounds(vertices, (int)submesh.FirstVertex, (int)submesh.VertexCount, out min, out max);
					return;
				}
			}

			if (AssetRipper.Core.Classes.Mesh.Mesh.HasVertexData(layout.Version))
			{
				if (AssetRipper.Core.Classes.Mesh.Mesh.IsOnlyVertexData(layout.Version))
				{
					FindMinMaxBounds(layout, mesh.VertexData, (int)submesh.FirstVertex, (int)submesh.VertexCount, out min, out max);
				}
				else
				{
					if (mesh.MeshCompression == MeshCompression.Off)
					{
						FindMinMaxBounds(layout, mesh.VertexData, (int)submesh.FirstVertex, (int)submesh.VertexCount, out min, out max);
					}
					else
					{
						FindMinMaxBounds(mesh.Vertices, (int)submesh.FirstVertex, (int)submesh.VertexCount, out min, out max);
					}
				}
			}
			else
			{
				FindMinMaxBounds(mesh.Vertices, (int)submesh.FirstVertex, (int)submesh.VertexCount, out min, out max);
			}
		}

		private static void FindMinMaxBounds(float[] vertexBuffer, int firstVertex, int vertexCount, out Vector3f min, out Vector3f max)
		{
			int offset = firstVertex * 3;
			min = new Vector3f(vertexBuffer[offset], vertexBuffer[offset + 1], vertexBuffer[offset + 2]);
			max = new Vector3f(vertexBuffer[offset], vertexBuffer[offset + 1], vertexBuffer[offset + 2]);
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

		private static void FindMinMaxBounds(Vector3f[] vertices, int firstVertex, int vertexCount, out Vector3f min, out Vector3f max)
		{
			min = vertices[firstVertex];
			max = vertices[firstVertex];
			int end = firstVertex + vertexCount;
			for (int i = firstVertex; i < end; i++)
			{
				Vector3f vertex = vertices[i];
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

		private static void FindMinMaxBounds(LayoutInfo layout, VertexData vertexData, int firstVertex, int vertexCount, out Vector3f min, out Vector3f max)
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
