using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.CompositeCollider2Ds
{
	public struct IntPoint : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			X = reader.ReadInt64();
			Y = reader.ReadInt64();
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
