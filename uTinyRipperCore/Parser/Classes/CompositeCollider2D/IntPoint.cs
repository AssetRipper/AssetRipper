using uTinyRipper.Converters;
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
			node.Add(XName, X);
			node.Add(YName, Y);
			return node;
		}

		public long X { get; set; }
		public long Y { get; set; }

		public const string XName = "X";
		public const string YName = "Y";
	}
}
