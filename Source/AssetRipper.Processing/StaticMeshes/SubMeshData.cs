using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;

namespace AssetRipper.Processing.StaticMeshes;

/// <summary>
/// 
/// </summary>
/// <param name="BaseVertex"></param>
/// <param name="FirstIndex">Offset in the index buffer.</param>
/// <param name="FirstVertex">Offset in the vertex list.</param>
/// <param name="IndexCount"></param>
/// <param name="TriangleCount"></param>
/// <param name="VertexCount"></param>
/// <param name="Topology"></param>
/// <param name="LocalBounds"></param>
public record struct SubMeshData(
	uint BaseVertex,
	int FirstIndex,
	int FirstVertex,
	int IndexCount,
	int TriangleCount,
	int VertexCount,
	MeshTopology Topology,
	Bounds LocalBounds)
{
	public static SubMeshData Create(ISubMesh subMesh, IndexFormat indexFormat)
	{
		return new SubMeshData(
			subMesh.BaseVertex,
			(int)subMesh.FirstByte / indexFormat.ToSize(),
			(int)subMesh.FirstVertex,
			(int)subMesh.IndexCount,
			(int)subMesh.TriangleCount,
			(int)subMesh.VertexCount,
			subMesh.GetTopology(),
			subMesh.LocalAABB);
	}

	public readonly void CopyTo(ISubMesh destination, IndexFormat indexFormat)
	{
		destination.BaseVertex = BaseVertex;
		destination.FirstByte = (uint)(FirstIndex * indexFormat.ToSize());
		destination.FirstVertex = (uint)FirstVertex;
		destination.IndexCount = (uint)IndexCount;
		destination.TriangleCount = (uint)TriangleCount;
		destination.VertexCount = (uint)VertexCount;
		destination.SetTopology(Topology);
		LocalBounds.CopyTo(destination.LocalAABB);
	}
}
