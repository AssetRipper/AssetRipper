using AssetRipper.Converters.Project;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;
using AssetRipper.YAML.Extensions;

namespace AssetRipper.Parser.Classes.Avatar
{
	public struct Hand : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			HandBoneIndex = reader.ReadInt32Array();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(HandBoneIndexName, HandBoneIndex.ExportYAML(true));
			return node;
		}

		public int[] HandBoneIndex { get; set; }

		public const string HandBoneIndexName = "m_HandBoneIndex";
	}
}
