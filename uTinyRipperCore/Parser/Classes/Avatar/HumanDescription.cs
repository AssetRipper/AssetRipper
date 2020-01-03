using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Avatars
{
	public struct HumanDescription : IAssetReadable, IYAMLExportable
	{
		public static int ToSerializedVersion(Version version)
		{
			// TODO:
			return 3;
		}

		public void Read(AssetReader reader)
		{
			Human = reader.ReadAssetArray<HumanBone>();
			Skeleton = reader.ReadAssetArray<SkeletonBone>();
			ArmTwist = reader.ReadSingle();
			ForeArmTwist = reader.ReadSingle();
			UpperLegTwist = reader.ReadSingle();
			LegTwist = reader.ReadSingle();
			ArmStretch = reader.ReadSingle();
			LegStretch = reader.ReadSingle();
			FeetSpacing = reader.ReadSingle();
			GlobalScale = reader.ReadSingle();
			RootMotionBoneName = reader.ReadString();
			HasTranslationDoF = reader.ReadBoolean();
			HasExtraRoot = reader.ReadBoolean();
			SkeletonHasParents = reader.ReadBoolean();
			reader.AlignStream();
			
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(HumanName, Human.ExportYAML(container));
			node.Add(SkeletonName, Skeleton.ExportYAML(container));
			node.Add(ArmTwistName, ArmTwist);
			node.Add(ForeArmTwistName, ForeArmTwist);
			node.Add(UpperLegTwistName, UpperLegTwist);
			node.Add(LegTwistName, LegTwist);
			node.Add(ArmStretchName, ArmStretch);
			node.Add(LegStretchName, LegStretch);
			node.Add(FeetSpacingName, FeetSpacing);
			node.Add(GlobalScaleName, GlobalScale);
			node.Add(RootMotionBoneNameName, RootMotionBoneName);
			node.Add(HasTranslationDoFName, HasTranslationDoF);
			node.Add(HasExtraRootName, HasExtraRoot);
			node.Add(SkeletonHasParentsName, SkeletonHasParents);
			return node;
		}

		public HumanBone[] Human { get; set; }
		public SkeletonBone[] Skeleton { get; set; }
		public float ArmTwist { get; set; }
		public float ForeArmTwist { get; set; }
		public float UpperLegTwist { get; set; }
		public float LegTwist { get; set; }
		public float ArmStretch { get; set; }
		public float LegStretch { get; set; }
		public float FeetSpacing { get; set; }
		public float GlobalScale { get; set; }
		public string RootMotionBoneName { get; set; }
		public bool HasTranslationDoF { get; set; }
		public bool HasExtraRoot { get; set; }
		public bool SkeletonHasParents { get; set; }

		public const string HumanName = "m_Human";
		public const string SkeletonName = "m_Skeleton";
		public const string ArmTwistName = "m_ArmTwist";
		public const string ForeArmTwistName = "m_ForeArmTwist";
		public const string UpperLegTwistName = "m_UpperLegTwist";
		public const string LegTwistName = "m_LegTwist";
		public const string ArmStretchName = "m_ArmStretch";
		public const string LegStretchName = "m_LegStretch";
		public const string FeetSpacingName = "m_FeetSpacing";
		public const string GlobalScaleName = "m_GlobalScale";
		public const string RootMotionBoneNameName = "m_RootMotionBoneName";
		public const string HasTranslationDoFName = "m_HasTranslationDoF";
		public const string HasExtraRootName = "m_HasExtraRoot";
		public const string SkeletonHasParentsName = "m_SkeletonHasParents";
	}
}
