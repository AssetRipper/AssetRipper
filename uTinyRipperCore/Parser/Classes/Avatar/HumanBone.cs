using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Avatars
{
	public struct HumanBone : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			BoneName = reader.ReadString();
			HumanName = reader.ReadString();
			Limit.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(BoneNameName, BoneName);
			node.Add(HumanNameName, HumanName);
			node.Add(LimitName, Limit.ExportYAML(container));
			return node;
		}

		public string BoneName { get; private set; }
		public string HumanName { get; private set; }

		public const string BoneNameName = "m_BoneName";
		public const string HumanNameName = "m_HumanName";
		public const string LimitName = "m_Limit";

		public SkeletonBoneLimit Limit;
	}
}
