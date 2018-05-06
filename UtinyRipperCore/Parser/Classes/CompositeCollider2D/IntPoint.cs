using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.CompositeCollider2Ds
{
	public struct IntPoint : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			X = stream.ReadInt64();
			Y = stream.ReadInt64();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("X", X);
			node.Add("Y", Y);
			return node;
		}

		public long X { get; private set; }
		public long Y { get; private set; }
	}
}
