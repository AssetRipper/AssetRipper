using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.NavMeshDatas
{
	public struct HeightMeshData : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_vertices = reader.ReadAssetArray<Vector3f>();
			m_indices = reader.ReadInt32Array();
			Bounds.Read(reader);
			m_nodes = reader.ReadAssetArray<HeightMeshBVNode>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(VerticesName, Vertices.ExportYAML(container));
			node.Add(IndicesName, Indices.ExportYAML(true));
			node.Add(BoundsName, Bounds.ExportYAML(container));
			node.Add(NodesName, Nodes.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<Vector3f> Vertices => m_vertices;
		public IReadOnlyList<int> Indices => m_indices;
		public IReadOnlyList<HeightMeshBVNode> Nodes => m_nodes;

		public const string VerticesName = "m_Vertices";
		public const string IndicesName = "m_Indices";
		public const string BoundsName = "m_Bounds";
		public const string NodesName = "m_Nodes";

		public AABB Bounds;

		private Vector3f[] m_vertices;
		private int[] m_indices;
		private HeightMeshBVNode[] m_nodes;
	}
}
