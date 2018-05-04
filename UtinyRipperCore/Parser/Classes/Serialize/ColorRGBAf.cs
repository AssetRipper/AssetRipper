using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct ColorRGBAf : IAssetReadable, IYAMLExportable
	{
		public ColorRGBAf(ColorRGBA32 color):
			this(color.RGBA)
		{
		}

		public ColorRGBAf(uint value32)
		{
			R = (value32 & 0xFF000000 >> 24) / 255.0f;
			G = (value32 & 0x00FF0000 >> 16) / 255.0f;
			B = (value32 & 0x0000FF00 >> 8) / 255.0f;
			A = (value32 & 0x000000FF >> 0) / 255.0f;
		}

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
