using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Shaders
{
	public struct KeywordTargetInfo : IAssetReadable, IYAMLExportable
	{
		public KeywordTargetInfo(string name, int requirements)
		{
			KeywordName = name;
			Requirements = requirements;
		}

		public void Read(AssetReader reader)
		{
			KeywordName = reader.ReadString();
			Requirements = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(KeywordNameName, KeywordName);
			node.Add(RequirementsName, Requirements);
			return node;
		}

		public override string ToString()
		{
			return KeywordName == null ? base.ToString() : $"{KeywordName}:{Requirements}";
		}

		public string KeywordName { get; private set; }
		public int Requirements { get; private set; }

		public const string KeywordNameName = "keywordName";
		public const string RequirementsName = "requirements";
	}
}
