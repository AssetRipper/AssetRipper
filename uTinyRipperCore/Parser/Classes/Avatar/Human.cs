using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes.Misc;
using uTinyRipper;

namespace uTinyRipper.Classes.Avatars
{
	public struct Human : IAssetReadable, IYAMLExportable
	{
		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5, 6, 0, VersionType.Patch, 2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 2018.2
		/// </summary>
		public static bool HasHandles(Version version) => version.IsLess(2018, 2);
		/// <summary>
		/// Less than 2018.2
		/// </summary>
		public static bool HasColliderIndex(Version version) => version.IsLess(2018, 2);
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool HasHasTDoF(Version version) => version.IsGreaterEqual(5, 2);

		public void Read(AssetReader reader)
		{
			RootX.Read(reader);
			Skeleton.Read(reader);
			SkeletonPose.Read(reader);
			LeftHand.Read(reader);
			RightHand.Read(reader);
			if (HasHandles(reader.Version))
			{
				Handles = reader.ReadAssetArray<Handle>();
				ColliderArray = reader.ReadAssetArray<Collider>();
			}

			int[] humanBoneIndex = reader.ReadInt32Array();
			HumanBoneIndex = UpdateBoneArray(humanBoneIndex, reader.Version);
			float[] humanBoneMass = reader.ReadSingleArray();
			HumanBoneMass = UpdateBoneArray(humanBoneMass, reader.Version);
			if (HasColliderIndex(reader.Version))
			{
				int[] colliderIndex = reader.ReadInt32Array();
				ColliderIndex = UpdateBoneArray(colliderIndex, reader.Version);
			}

			Scale = reader.ReadSingle();
			ArmTwist = reader.ReadSingle();
			ForeArmTwist = reader.ReadSingle();
			UpperLegTwist = reader.ReadSingle();
			LegTwist = reader.ReadSingle();
			ArmStretch = reader.ReadSingle();
			LegStretch = reader.ReadSingle();
			FeetSpacing = reader.ReadSingle();
			HasLeftHand = reader.ReadBoolean();
			HasRightHand = reader.ReadBoolean();
			if (HasHasTDoF(reader.Version))
			{
				HasTDoF = reader.ReadBoolean();
			}
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(RootXName, RootX.ExportYAML(container));
			node.Add(SkeletonName, Skeleton.ExportYAML(container));
			node.Add(SkeletonPoseName, SkeletonPose.ExportYAML(container));
			node.Add(LeftHandName, LeftHand.ExportYAML(container));
			node.Add(RightHandName, RightHand.ExportYAML(container));
			node.Add(HandlesName, GetExportHandles(container.Version).ExportYAML(container));
			node.Add(ColliderArrayName, GetExportColliderArray(container.Version).ExportYAML(container));
			node.Add(HumanBoneIndexName, HumanBoneIndex.ExportYAML(true));
			node.Add(HumanBoneMassName, HumanBoneMass.ExportYAML());
			node.Add(ColliderIndexName, GetExportColliderIndex(container.Version).ExportYAML(true));
			node.Add(ScaleName, Scale);
			node.Add(ArmTwistName, ArmTwist);
			node.Add(ForeArmTwistName, ForeArmTwist);
			node.Add(UpperLegTwistName, UpperLegTwist);
			node.Add(LegTwistName, LegTwist);
			node.Add(ArmStretchName, ArmStretch);
			node.Add(LegStretchName, LegStretch);
			node.Add(FeetSpacingName, FeetSpacing);
			node.Add(HasLeftHandName, HasLeftHand);
			node.Add(HasRightHandName, HasRightHand);
			node.Add(HasTDoFName, HasTDoF);
			return node;
		}

		private int[] UpdateBoneArray(int[] array, Version version)
		{
			if (!BoneTypeExtensions.IsIncludeUpperChest(version))
			{
				int[] fixedArray = new int[array.Length + 1];
				BoneType bone;
				for (bone = BoneType.Hips; bone < BoneType.UpperChest; bone++)
				{
					fixedArray[(int)bone] = array[(int)bone];
				}
				fixedArray[(int)bone] = -1;
				for (bone = BoneType.UpperChest + 1; bone < BoneType.Last; bone++)
				{
					fixedArray[(int)bone] = array[(int)bone - 1];
				}
				return fixedArray;
			}
			return array;
		}
		private float[] UpdateBoneArray(float[] array, Version version)
		{
			if (!BoneTypeExtensions.IsIncludeUpperChest(version))
			{
				float[] fixedArray = new float[array.Length + 1];
				BoneType bone;
				for (bone = BoneType.Hips; bone < BoneType.UpperChest; bone++)
				{
					fixedArray[(int)bone] = array[(int)bone];
				}
				fixedArray[(int)bone] = 0.0f;
				for (bone = BoneType.UpperChest + 1; bone < BoneType.Last; bone++)
				{
					fixedArray[(int)bone] = array[(int)bone - 1];
				}
				return fixedArray;
			}
			return array;
		}

		private IReadOnlyList<Handle> GetExportHandles(Version version)
		{
			return HasHandles(version) ? Handles : System.Array.Empty<Handle>();
		}
		private IReadOnlyList<Collider> GetExportColliderArray(Version version)
		{
			return HasHandles(version) ? ColliderArray : System.Array.Empty<Collider>();
		}
		private IReadOnlyList<int> GetExportColliderIndex(Version version)
		{
			return HasColliderIndex(version) ? ColliderIndex : System.Array.Empty<int>();
		}

		public Handle[] Handles { get; set; }
		public Collider[] ColliderArray { get; set; }
		public int[] HumanBoneIndex { get; set; }
		public float[] HumanBoneMass { get; set; }
		public int[] ColliderIndex { get; set; }
		public float Scale { get; set; }
		public float ArmTwist { get; set; }
		public float ForeArmTwist { get; set; }
		public float UpperLegTwist { get; set; }
		public float LegTwist { get; set; }
		public float ArmStretch { get; set; }
		public float LegStretch { get; set; }
		public float FeetSpacing { get; set; }
		public bool HasLeftHand { get; set; }
		public bool HasRightHand { get; set; }
		public bool HasTDoF { get; set; }

		public const string RootXName = "m_RootX";
		public const string SkeletonName = "m_Skeleton";
		public const string SkeletonPoseName = "m_SkeletonPose";
		public const string LeftHandName = "m_LeftHand";
		public const string RightHandName = "m_RightHand";
		public const string HandlesName = "m_Handles";
		public const string ColliderArrayName = "m_ColliderArray";
		public const string HumanBoneIndexName = "m_HumanBoneIndex";
		public const string HumanBoneMassName = "m_HumanBoneMass";
		public const string ColliderIndexName = "m_ColliderIndex";
		public const string ScaleName = "m_Scale";
		public const string ArmTwistName = "m_ArmTwist";
		public const string ForeArmTwistName = "m_ForeArmTwist";
		public const string UpperLegTwistName = "m_UpperLegTwist";
		public const string LegTwistName = "m_LegTwist";
		public const string ArmStretchName = "m_ArmStretch";
		public const string LegStretchName = "m_LegStretch";
		public const string FeetSpacingName = "m_FeetSpacing";
		public const string HasLeftHandName = "m_HasLeftHand";
		public const string HasRightHandName = "m_HasRightHand";
		public const string HasTDoFName = "m_HasTDoF";

		public XForm RootX;
		public OffsetPtr<Skeleton> Skeleton;
		public OffsetPtr<SkeletonPose> SkeletonPose;
		public OffsetPtr<Hand> LeftHand;
		public OffsetPtr<Hand> RightHand;
	}
}
