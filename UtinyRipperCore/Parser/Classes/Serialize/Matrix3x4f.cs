using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct Matrix3x4f : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			E00 = stream.ReadSingle();
			E01 = stream.ReadSingle();
			E02 = stream.ReadSingle();
			E03 = stream.ReadSingle();
			E10 = stream.ReadSingle();
			E11 = stream.ReadSingle();
			E12 = stream.ReadSingle();
			E13 = stream.ReadSingle();
			E20 = stream.ReadSingle();
			E21 = stream.ReadSingle();
			E22 = stream.ReadSingle();
			E23 = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("e00", E00);
			node.Add("e01", E01);
			node.Add("e02", E02);
			node.Add("e03", E03);
			node.Add("e10", E10);
			node.Add("e11", E11);
			node.Add("e12", E12);
			node.Add("e13", E13);
			node.Add("e20", E20);
			node.Add("e21", E21);
			node.Add("e22", E22);
			node.Add("e23", E23);
			return node;
		}

		public float E00 { get; private set; }
		public float E01 { get; private set; }
		public float E02 { get; private set; }
		public float E03 { get; private set; }
		public float E10 { get; private set; }
		public float E11 { get; private set; }
		public float E12 { get; private set; }
		public float E13 { get; private set; }
		public float E20 { get; private set; }
		public float E21 { get; private set; }
		public float E22 { get; private set; }
		public float E23 { get; private set; }
	}
}
