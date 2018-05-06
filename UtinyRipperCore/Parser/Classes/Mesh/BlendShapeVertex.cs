using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Meshes
{
	/// <summary>
	/// MeshBlendShapeVertex previously
	/// </summary>
	public struct BlendShapeVertex : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Position.Read(stream);
			Normal.Read(stream);
			Tangent.Read(stream);
			Index = stream.ReadUInt32();
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
