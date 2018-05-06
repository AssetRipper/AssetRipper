using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.NavMeshDatas
{
	public struct HeightMeshData : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			m_vertices = stream.ReadArray<Vector3f>();
			m_indices = stream.ReadInt32Array();
			Bounds.Read(stream);
			m_nodes = stream.ReadArray<HeightMeshBVNode>();
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
