using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.TerrainDatas
{
	public struct Shift : IAsset
	{
		public void Read(AssetReader reader)
		{
			X = reader.ReadUInt16();
			Y = reader.ReadUInt16();
			Flags = reader.ReadUInt16();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Flags);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(XName, X);
			node.Add(YName, Y);
			node.Add(FlagsName, Flags);
			return node;
		}

		public ushort X { get; set; }
		public ushort Y { get; set; }
		public ushort Flags { get; set; }

		public const string XName = "x";
		public const string YName = "y";
		public const string FlagsName = "flags";
	}
}
