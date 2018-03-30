using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct ColorRGBAf : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			R = stream.ReadSingle();
			G = stream.ReadSingle();
			B = stream.ReadSingle();
			A = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add("r", R);
			node.Add("g", G);
			node.Add("b", B);
			node.Add("a", A);
			return node;
		}

		public float R { get; private set; }
		public float G { get; private set; }
		public float B { get; private set; }
		public float A { get; private set; }
	}
}
