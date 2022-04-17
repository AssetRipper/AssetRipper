using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader
{
	public sealed class KeywordTargetInfo : IAssetReadable, IYamlExportable
	{
		public KeywordTargetInfo() { }

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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(KeywordNameName, KeywordName);
			node.Add(RequirementsName, Requirements);
			return node;
		}

		public override string ToString()
		{
			return KeywordName == null ? base.ToString() : $"{KeywordName}:{Requirements}";
		}

		public string KeywordName { get; set; }
		public int Requirements { get; set; }

		public const string KeywordNameName = "keywordName";
		public const string RequirementsName = "requirements";
	}
}
