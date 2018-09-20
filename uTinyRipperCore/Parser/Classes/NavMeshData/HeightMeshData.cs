using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.NavMeshDatas
{
	public struct HeightMeshData : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_vertices = reader.ReadArray<Vector3f>();
			m_indices = reader.ReadInt32Array();
			Bounds.Read(reader);
			m_nodes = reader.ReadArray<HeightMeshBVNode>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Vertices", Vertices.ExportYAML(container));
			node.Add("m_Indices", Indices.ExportYAML(true));
			node.Add("m_Bounds", Bounds.ExportYAML(container));
			node.Add("m_Nodes", Nodes.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<Vector3f> Vertices => m_vertices;
		public IReadOnlyList<int> Indices => m_indices;
		public IReadOnlyList<HeightMeshBVNode> Nodes => m_nodes;

		public AABB Bounds;

		private Vector3f[] m_vertices;
		private int[] m_indices;
		private HeightMeshBVNode[] m_nodes;
	}
}
