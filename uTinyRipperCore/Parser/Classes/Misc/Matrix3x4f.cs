using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Misc
{
	public struct Matrix3x4f : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			E00 = reader.ReadSingle();
			E01 = reader.ReadSingle();
			E02 = reader.ReadSingle();
			E03 = reader.ReadSingle();
			E10 = reader.ReadSingle();
			E11 = reader.ReadSingle();
			E12 = reader.ReadSingle();
			E13 = reader.ReadSingle();
			E20 = reader.ReadSingle();
			E21 = reader.ReadSingle();
			E22 = reader.ReadSingle();
			E23 = reader.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(E00Name, E00);
			node.Add(E01Name, E01);
			node.Add(E02Name, E02);
			node.Add(E03Name, E03);
			node.Add(E10Name, E10);
			node.Add(E11Name, E11);
			node.Add(E12Name, E12);
			node.Add(E13Name, E13);
			node.Add(E20Name, E20);
			node.Add(E21Name, E21);
			node.Add(E22Name, E22);
			node.Add(E23Name, E23);
			return node;
		}

		public float E00 { get; set; }
		public float E01 { get; set; }
		public float E02 { get; set; }
		public float E03 { get; set; }
		public float E10 { get; set; }
		public float E11 { get; set; }
		public float E12 { get; set; }
		public float E13 { get; set; }
		public float E20 { get; set; }
		public float E21 { get; set; }
		public float E22 { get; set; }
		public float E23 { get; set; }

		public const string E00Name = "e00";
		public const string E01Name = "e01";
		public const string E02Name = "e02";
		public const string E03Name = "e03";
		public const string E10Name = "e10";
		public const string E11Name = "e11";
		public const string E12Name = "e12";
		public const string E13Name = "e13";
		public const string E20Name = "e20";
		public const string E21Name = "e21";
		public const string E22Name = "e22";
		public const string E23Name = "e23";
	}
}
