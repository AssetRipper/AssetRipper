using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Misc
{
	/// <summary>
	/// Real name is 'int2_storage'
	/// </summary>
	public struct Int2Storage : IAsset
	{
		public void Read(AssetReader reader)
		{
			X = reader.ReadInt32();
			Y = reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add(XName, X);
			node.Add(YName, Y);
			return node;
		}

		public override string ToString()
		{
			return $"[{X}, {Y}]";
		}

		public int X { get; set; }
		public int Y { get; set; }

		public const string XName = "x";
		public const string YName = "y";
	}
}
