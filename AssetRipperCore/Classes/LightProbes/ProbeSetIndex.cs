using AssetRipper.Project;
using AssetRipper.Classes.Misc;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;

namespace AssetRipper.Classes.LightProbes
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
