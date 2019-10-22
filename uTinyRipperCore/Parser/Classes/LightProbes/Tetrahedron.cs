using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.LightProbess
{
	public struct Tetrahedron : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Indices0 = reader.ReadInt32();
			Indices1 = reader.ReadInt32();
			Indices2 = reader.ReadInt32();
			Indices3 = reader.ReadInt32();
			Neighbors0 = reader.ReadInt32();
			Neighbors1 = reader.ReadInt32();
			Neighbors2 = reader.ReadInt32();
			Neighbors3 = reader.ReadInt32();
			Matrix.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(IndicesName + "[0]", Indices0);
			node.Add(IndicesName + "[1]", Indices1);
			node.Add(IndicesName + "[2]", Indices2);
			node.Add(IndicesName + "[3]", Indices3);
			node.Add(NeighborsName + "[0]", Neighbors0);
			node.Add(NeighborsName + "[1]", Neighbors1);
			node.Add(NeighborsName + "[2]", Neighbors2);
			node.Add(NeighborsName + "[3]", Neighbors3);
			node.Add(MatrixName, Matrix.ExportYAML(container));
			return node;
		}

		public int Indices0 { get; private set; }
		public int Indices1 { get; private set; }
		public int Indices2 { get; private set; }
		public int Indices3 { get; private set; }
		public int Neighbors0 { get; private set; }
		public int Neighbors1 { get; private set; }
		public int Neighbors2 { get; private set; }
		public int Neighbors3 { get; private set; }

		public const string IndicesName = "indices";
		public const string NeighborsName = "neighbors";
		public const string MatrixName = "matrix";

		public Matrix3x4f Matrix;
	}
}
