using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AvatarMask
{
	public sealed class TransformMaskElement : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Path = reader.ReadString();
			Weight = reader.ReadSingle();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(PathName, Path);
			node.Add(WeightName, Weight);
			return node;
		}

		public string Path { get; set; }
		public float Weight { get; set; }

		public const string PathName = "m_Path";
		public const string WeightName = "m_Weight";
	}
}
