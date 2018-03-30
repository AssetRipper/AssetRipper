using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct Vector4f : IAssetReadable, IYAMLExportable
	{
		public Vector4f(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public void Read(AssetStream stream)
		{
			X = stream.ReadSingle();
			Y = stream.ReadSingle();
			Z = stream.ReadSingle();
			W = stream.ReadSingle();
		}

		public void Read3(AssetStream stream)
		{
			X = stream.ReadSingle();
			Y = stream.ReadSingle();
			Z = stream.ReadSingle();
		}
		
		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode(MappingStyle.Flow);
			node.Add("x", X);
			node.Add("y", Y);
			node.Add("z", Z);
			node.Add("w", W);
			return node;
		}

		public YAMLNode ExportYAML3(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode(MappingStyle.Flow);
			node.Add("x", X);
			node.Add("y", Y);
			node.Add("z", Z);
			return node;
		}

		public override string ToString()
		{
			return $"[{X:0.00}, {Y:0.00}, {Z:0.00}, {W:0.00}]";
		}

		public float X { get; private set; }
		public float Y { get; private set; }
		public float Z { get; private set; }
		public float W { get; private set; }
	}
}
