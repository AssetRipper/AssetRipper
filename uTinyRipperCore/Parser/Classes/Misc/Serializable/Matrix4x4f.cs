using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public struct Matrix4x4f : IAsset
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
			E30 = reader.ReadSingle();
			E31 = reader.ReadSingle();
			E32 = reader.ReadSingle();
			E33 = reader.ReadSingle();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(E00);
			writer.Write(E01);
			writer.Write(E02);
			writer.Write(E03);
			writer.Write(E10);
			writer.Write(E11);
			writer.Write(E12);
			writer.Write(E13);
			writer.Write(E20);
			writer.Write(E21);
			writer.Write(E22);
			writer.Write(E23);
			writer.Write(E30);
			writer.Write(E31);
			writer.Write(E32);
			writer.Write(E33);
		}
		
		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			Matrix4x4fLayout layout = container.ExportLayout.Serialized.Matrix4x4f;
			node.Add(layout.E00Name, E00);
			node.Add(layout.E01Name, E01);
			node.Add(layout.E02Name, E02);
			node.Add(layout.E03Name, E03);
			node.Add(layout.E10Name, E10);
			node.Add(layout.E11Name, E11);
			node.Add(layout.E12Name, E12);
			node.Add(layout.E13Name, E13);
			node.Add(layout.E20Name, E20);
			node.Add(layout.E21Name, E21);
			node.Add(layout.E22Name, E22);
			node.Add(layout.E23Name, E23);
			node.Add(layout.E30Name, E30);
			node.Add(layout.E31Name, E31);
			node.Add(layout.E32Name, E32);
			node.Add(layout.E33Name, E33);
			return node;
		}

		public static Matrix4x4f Identity => new Matrix4x4f { E00 = 1.0f, E11 = 1.0f, E22 = 1.0f, E33 = 1.0f };

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
		public float E30 { get; set; }
		public float E31 { get; set; }
		public float E32 { get; set; }
		public float E33 { get; set; }
	}
}
