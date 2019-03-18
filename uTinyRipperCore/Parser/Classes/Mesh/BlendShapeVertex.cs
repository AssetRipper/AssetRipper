using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	/// <summary>
	/// MeshBlendShapeVertex previously
	/// </summary>
	public struct BlendShapeVertex : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Position.Read(reader);
			Normal.Read(reader);
			Tangent.Read(reader);
			Index = reader.ReadUInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("vertex", Position.ExportYAML(container));
			node.Add("normal", Normal.ExportYAML(container));
			node.Add("tangent", Tangent.ExportYAML(container));
			node.Add("index", Index);
			return node;
		}

		public uint Index { get; private set; }

		public Vector3f Position;
		public Vector3f Normal;
		public Vector3f Tangent;
	}
}
