using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.LightProbes
{
	public sealed class ProbeSetIndex : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Hash.Read(reader);
			Offset = reader.ReadInt32();
			Size = reader.ReadInt32();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(HashName, Hash.ExportYaml(container));
			node.Add(OffsetName, Offset);
			node.Add(SizeName, Size);
			return node;
		}

		public int Offset { get; set; }
		public int Size { get; set; }

		public const string HashName = "m_Hash";
		public const string OffsetName = "m_Offset";
		public const string SizeName = "m_Size";

		public Hash128 Hash = new();
	}
}
