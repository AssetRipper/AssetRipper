using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.LightProbess
{
	public struct Tetrahedron : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Indices_0 = reader.ReadInt32();
			Indices_1 = reader.ReadInt32();
			Indices_2 = reader.ReadInt32();
			Indices_3 = reader.ReadInt32();
			Neighbors_0 = reader.ReadInt32();
			Neighbors_1 = reader.ReadInt32();
			Neighbors_2 = reader.ReadInt32();
			Neighbors_3 = reader.ReadInt32();
			Matrix.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("indices[0]", Indices_0);
			node.Add("indices[1]", Indices_1);
			node.Add("indices[2]", Indices_2);
			node.Add("indices[3]", Indices_3);
			node.Add("neighbors[0]", Neighbors_0);
			node.Add("neighbors[1]", Neighbors_1);
			node.Add("neighbors[2]", Neighbors_2);
			node.Add("neighbors[3]", Neighbors_3);
			node.Add("matrix", Matrix.ExportYAML(container));
			return node;
		}

		public int Indices_0 { get; private set; }
		public int Indices_1 { get; private set; }
		public int Indices_2 { get; private set; }
		public int Indices_3 { get; private set; }
		public int Neighbors_0 { get; private set; }
		public int Neighbors_1 { get; private set; }
		public int Neighbors_2 { get; private set; }
		public int Neighbors_3 { get; private set; }

		public Matrix3x4f Matrix;
	}
}
