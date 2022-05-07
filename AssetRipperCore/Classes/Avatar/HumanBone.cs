﻿using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Avatar
{
	public sealed class HumanBone : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			BoneName = reader.ReadString();
			HumanName = reader.ReadString();
			Limit.Read(reader);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(BoneNameName, BoneName);
			node.Add(HumanNameName, HumanName);
			node.Add(LimitName, Limit.ExportYaml(container));
			return node;
		}

		public string BoneName { get; set; }
		public string HumanName { get; set; }

		public const string BoneNameName = "m_BoneName";
		public const string HumanNameName = "m_HumanName";
		public const string LimitName = "m_Limit";

		public SkeletonBoneLimit Limit = new();
	}
}
