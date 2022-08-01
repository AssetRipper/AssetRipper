using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using System.Collections.Generic;

namespace AssetRipper.Library.Exporters.Meshes
{
	internal static class TriangleProcessor
	{
		public static List<uint> ReadIndices(IMesh mesh, uint[] processedIndexBuffer)
		{
			List<uint> indices = new();

			foreach (ISubMesh subMesh in mesh.SubMeshes_C43)
			{
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
						}
						else
						{
							indices.Add(a);
							indices.Add(b);
						}
						indices.Add(c);
						triIndex += 3;
					}
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
					}
				}
			}
			
			return indices;
		}
	}
}
