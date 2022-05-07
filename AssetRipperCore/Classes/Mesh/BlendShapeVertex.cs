using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Mesh
{
	public sealed class BlendShapeVertex : IBlendShapeVertex
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(VertexName, Vertex.ExportYaml(container));
			node.Add(NormalName, Normal.ExportYaml(container));
			node.Add(TangentName, Tangent.ExportYaml(container));
			node.Add(IndexName, Index);
			return node;
		}

		public uint Index { get; set; }
		public IVector3f Vertex { get; } = new Vector3f();
		public IVector3f Normal { get; } = new Vector3f();
		public IVector3f Tangent { get; } = new Vector3f();

		public const string VertexName = "vertex";
		public const string NormalName = "normal";
		public const string TangentName = "tangent";
		public const string IndexName = "index";
	}
}
