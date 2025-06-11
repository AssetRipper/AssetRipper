using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SubMeshExtensions
{
	extension(ISubMesh subMesh)
	{
		/// <summary>
		/// For versions &lt; 4, IsTriStrip is used here instead.<br/>
		/// For it, 0 cooresponds to <see cref="MeshTopology.Triangles"/>,<br/>
		/// and non-zero cooresponds to <see cref="MeshTopology.TriangleStrip"/>.<br/>
		/// This conveniently matches the <see cref="MeshTopology"/> enumeration.
		/// </summary>
		public MeshTopology GetTopology()
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

		public void SetTopology(MeshTopology topology)
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

		public uint GetFirstIndex(bool is16BitIndices)
		{
			return is16BitIndices ? subMesh.FirstByte / sizeof(ushort) : subMesh.FirstByte / sizeof(uint);
		}

		public void SetFirstIndex(bool is16BitIndices, uint firstIndex)
		{
			subMesh.FirstByte = is16BitIndices ? firstIndex * sizeof(ushort) : firstIndex * sizeof(uint);
		}
	}
}
