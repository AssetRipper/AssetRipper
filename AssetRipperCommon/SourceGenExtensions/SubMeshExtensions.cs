using AssetRipper.Core.Classes.Mesh;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;

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
	}
}
