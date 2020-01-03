using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
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
			node.Add(Indices0Name, Indices0);
			node.Add(Indices1Name, Indices1);
			node.Add(Indices2Name, Indices2);
			node.Add(Indices3Name, Indices3);
			node.Add(Neighbors0Name, Neighbors0);
			node.Add(Neighbors1Name, Neighbors1);
			node.Add(Neighbors2Name, Neighbors2);
			node.Add(Neighbors3Name, Neighbors3);
			node.Add(MatrixName, Matrix.ExportYAML(container));
			return node;
		}

		public int Indices0 { get; set; }
		public int Indices1 { get; set; }
		public int Indices2 { get; set; }
		public int Indices3 { get; set; }
		public int Neighbors0 { get; set; }
		public int Neighbors1 { get; set; }
		public int Neighbors2 { get; set; }
		public int Neighbors3 { get; set; }

		public const string Indices0Name = "indices[0]";
		public const string Indices1Name = "indices[1]";
		public const string Indices2Name = "indices[2]";
		public const string Indices3Name = "indices[3]";
		public const string Neighbors0Name = "neighbors[0]";
		public const string Neighbors1Name = "neighbors[1]";
		public const string Neighbors2Name = "neighbors[2]";
		public const string Neighbors3Name = "neighbors[3]";
		public const string MatrixName = "matrix";

		public Matrix3x4f Matrix;
	}
}
