using AssetRipper.Core.Classes.Misc.Serializable.Boundaries;
using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Mesh
{
	public interface ISubMesh : IAsset
	{
		/// <summary>
		/// Offset in index buffer
		/// </summary>
		uint FirstByte { get; set; }
		uint IndexCount { get; set; }
		/// <summary>
		/// For versions &lt; 4, IsTriStrip is used here instead.<br/>
		/// For it, 0 cooresponds to <see cref="MeshTopology.Triangles"/>,<br/>
		/// and 1 cooresponds to <see cref="MeshTopology.TriangleStrip"/>.<br/>
		/// This conveniently matches the <see cref="MeshTopology"/> enumeration.
		/// </summary>
		MeshTopology Topology { get; set; }
		uint TriangleCount { get; set; }
		uint BaseVertex { get; set; }
		/// <summary>
		/// Offset in Vertices
		/// </summary>
		uint FirstVertex { get; set; }
		uint VertexCount { get; set; }
		IAABB LocalAABB { get; }
	}
}
