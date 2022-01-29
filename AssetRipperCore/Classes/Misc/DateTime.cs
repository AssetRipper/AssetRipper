using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Misc
{
	public sealed class DateTime : IAssetReadable, IYAMLExportable
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
			node.Add(HighSecondsName, HighSeconds);
			node.Add(FractionName, Fraction);
			node.Add(LowSecondsName, LowSeconds);
			return node;
		}

		public ushort HighSeconds { get; set; }
		public ushort Fraction { get; set; }
		public uint LowSeconds { get; set; }

		public const string HighSecondsName = "highSeconds";
		public const string FractionName = "fraction";
		public const string LowSecondsName = "lowSeconds";
	}
}
