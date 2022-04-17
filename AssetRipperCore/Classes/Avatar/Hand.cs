using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes.Avatar
{
	public sealed class Hand : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			HandBoneIndex = reader.ReadInt32Array();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(HandBoneIndexName, HandBoneIndex.ExportYaml(true));
			return node;
		}

		public int[] HandBoneIndex { get; set; }

		public const string HandBoneIndexName = "m_HandBoneIndex";
	}
}
