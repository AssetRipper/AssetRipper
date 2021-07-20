using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.Misc;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.YAML;

namespace AssetRipper.Parser.Classes.LightProbes
{
	public struct ProbeSetIndex : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Hash.Read(reader);
			Offset = reader.ReadInt32();
			Size = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(HashName, Hash.ExportYAML(container));
			node.Add(OffsetName, Offset);
			node.Add(SizeName, Size);
			return node;
		}

		public int Offset { get; set; }
		public int Size { get; set; }

		public const string HashName = "m_Hash";
		public const string OffsetName = "m_Offset";
		public const string SizeName = "m_Size";

		public Hash128 Hash;
	}
}
