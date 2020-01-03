using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	/// <summary>
	/// MeshBlendShapeVertex previously
	/// </summary>
	public struct BlendShapeVertex : IAsset
	{
		public void Read(AssetReader reader)
		{
			Vertex.Read(reader);
			Normal.Read(reader);
			Tangent.Read(reader);
			Index = reader.ReadUInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.WriteAsset(Vertex);
			writer.WriteAsset(Normal);
			writer.WriteAsset(Tangent);
			writer.Write(Index);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(VertexName, Vertex.ExportYAML(container));
			node.Add(NormalName, Normal.ExportYAML(container));
			node.Add(TangentName, Tangent.ExportYAML(container));
			node.Add(IndexName, Index);
			return node;
		}

		public uint Index { get; set; }

		public const string VertexName = "vertex";
		public const string NormalName = "normal";
		public const string TangentName = "tangent";
		public const string IndexName = "index";

		public Vector3f Vertex;
		public Vector3f Normal;
		public Vector3f Tangent;
	}
}
