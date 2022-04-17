using AssetRipper.Core.Classes.Misc.Serializable.Boundaries;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes.NavMeshData
{
	public sealed class HeightMeshData : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Vertices = reader.ReadAssetArray<Vector3f>();
			Indices = reader.ReadInt32Array();
			Bounds.Read(reader);
			Nodes = reader.ReadAssetArray<HeightMeshBVNode>();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(VerticesName, Vertices.ExportYaml(container));
			node.Add(IndicesName, Indices.ExportYaml(true));
			node.Add(BoundsName, Bounds.ExportYaml(container));
			node.Add(NodesName, Nodes.ExportYaml(container));
			return node;
		}

		public Vector3f[] Vertices { get; set; }
		public int[] Indices { get; set; }
		public HeightMeshBVNode[] Nodes { get; set; }

		public const string VerticesName = "m_Vertices";
		public const string IndicesName = "m_Indices";
		public const string BoundsName = "m_Bounds";
		public const string NodesName = "m_Nodes";

		public AABB Bounds = new AABB();
	}
}
