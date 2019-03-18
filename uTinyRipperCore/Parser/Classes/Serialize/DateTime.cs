using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public struct DateTime : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			HighSeconds = reader.ReadUInt16();
			Fraction = reader.ReadUInt16();
			LowSeconds = reader.ReadUInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("highSeconds", HighSeconds);
			node.Add("fraction", Fraction);
			node.Add("lowSeconds", LowSeconds);
			return node;
		}

		public ushort HighSeconds { get; private set; }
		public ushort Fraction { get; private set; }
		public uint LowSeconds { get; private set; }
	}
}
