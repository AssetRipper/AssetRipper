using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using System.IO;
using System.Linq;

namespace AssetRipper.Core.Converters.Mesh
{
	public static class LODConverter
	{
		public static byte[] GenerateIndexBuffer(IExportContainer container, LOD origin)
		{
			int indexCount = origin.MeshData.Sum(t => t.Faces.Length) * 3;
			int dataSize = indexCount * sizeof(ushort);
			byte[] buffer = new byte[dataSize];
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				EndianType endian = container.ExportPlatform == Platform.XBox360 ? EndianType.BigEndian : EndianType.LittleEndian;
				using EndianWriter writer = new EndianWriter(stream, endian);
				for (int i = 0; i < origin.MeshData.Length; i++)
				{
					MeshData meshData = origin.MeshData[i];
					for (int j = 0; j < meshData.Faces.Length; j++)
					{
						Face face = meshData.Faces[j];
						writer.Write(face.V1);
						writer.Write(face.V2);
						writer.Write(face.V3);
					}
				}
			}
			return buffer;
		}

		public static SubMesh[] GenerateSubMeshes(IExportContainer container, AssetRipper.Core.Classes.Mesh.Mesh instanceMesh, LOD origin)
		{
			int offset = 0;
			SubMesh[] instances = new SubMesh[origin.MeshData.Length];
			for (int i = 0; i < origin.MeshData.Length; i++)
			{
				MeshData meshData = origin.MeshData[i];
				SubMesh instance = new SubMesh();
				instance.FirstByte = (uint)offset;
#warning TODO: stripping
				int indexCount = meshData.Faces.Length * 3;
				instance.IndexCount = (uint)indexCount;
				instance.Topology = MeshTopology.Triangles;
				if (SubMesh.HasTriangleCount(container.ExportVersion))
				{
					instance.TriangleCount = (uint)meshData.Faces.Length;
				}
				if (SubMesh.HasVertex(container.ExportVersion))
				{
					SubMeshConverter.CalculateSubMeshVertexRangeAndBounds(container.ExportLayout, instanceMesh, instance);
				}
				instances[i] = instance;
				offset += indexCount * sizeof(ushort);
			}
			return instances;
		}
	}
}
