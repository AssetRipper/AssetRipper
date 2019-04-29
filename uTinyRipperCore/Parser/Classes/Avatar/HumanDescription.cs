using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Avatars
{
	public struct HumanDescription : IAssetReadable, IYAMLExportable
	{
		private static int GetSerializedVersion(Version version)
		{
			// TODO:
			return 3;
		}

		public void Read(AssetReader reader)
		{
			m_human = reader.ReadAssetArray<HumanBone>();
			m_skeleton = reader.ReadAssetArray<SkeletonBone>();
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
			reader.AlignStream(AlignType.Align4);
			
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
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

		public IReadOnlyList<HumanBone> Human => m_human;
		public IReadOnlyList<SkeletonBone> Skeleton => m_skeleton;
		public float ArmTwist { get; private set; }
		public float ForeArmTwist { get; private set; }
		public float UpperLegTwist { get; private set; }
		public float LegTwist { get; private set; }
		public float ArmStretch { get; private set; }
		public float LegStretch { get; private set; }
		public float FeetSpacing { get; private set; }
		public float GlobalScale { get; private set; }
		public string RootMotionBoneName { get; private set; }
		public bool HasTranslationDoF { get; private set; }
		public bool HasExtraRoot { get; private set; }
		public bool SkeletonHasParents { get; private set; }

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

		private HumanBone[] m_human;
		private SkeletonBone[] m_skeleton;
	}
}
