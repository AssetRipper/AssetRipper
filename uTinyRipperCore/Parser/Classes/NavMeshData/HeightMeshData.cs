using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper;

namespace uTinyRipper.Classes.NavMeshDatas
{
	public struct HeightMeshData : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Vertices = reader.ReadAssetArray<Vector3f>();
			Indices = reader.ReadInt32Array();
			Bounds.Read(reader);
			Nodes = reader.ReadAssetArray<HeightMeshBVNode>();
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

		public Vector3f[] Vertices { get; set; }
		public int[] Indices { get; set; }
		public HeightMeshBVNode[] Nodes { get; set; }

		public const string VerticesName = "m_Vertices";
		public const string IndicesName = "m_Indices";
		public const string BoundsName = "m_Bounds";
		public const string NodesName = "m_Nodes";

		public AABB Bounds;
	}
}
