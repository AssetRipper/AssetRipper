using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.Bones;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Avatar
{
	public sealed class Human : IAssetReadable, IYamlExportable
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(5, 6, 0, UnityVersionType.Patch, 2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 2018.2
		/// </summary>
		public static bool HasHandles(UnityVersion version) => version.IsLess(2018, 2);
		/// <summary>
		/// Less than 2018.2
		/// </summary>
		public static bool HasColliderIndex(UnityVersion version) => version.IsLess(2018, 2);
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool HasHasTDoF(UnityVersion version) => version.IsGreaterEqual(5, 2);

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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(RootXName, RootX.ExportYaml(container));
			node.Add(SkeletonName, Skeleton.ExportYaml(container));
			node.Add(SkeletonPoseName, SkeletonPose.ExportYaml(container));
			node.Add(LeftHandName, LeftHand.ExportYaml(container));
			node.Add(RightHandName, RightHand.ExportYaml(container));
			node.Add(HandlesName, GetExportHandles(container.Version).ExportYaml(container));
			node.Add(ColliderArrayName, GetExportColliderArray(container.Version).ExportYaml(container));
			node.Add(HumanBoneIndexName, HumanBoneIndex.ExportYaml(true));
			node.Add(HumanBoneMassName, HumanBoneMass.ExportYaml());
			node.Add(ColliderIndexName, GetExportColliderIndex(container.Version).ExportYaml(true));
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

		private int[] UpdateBoneArray(int[] array, UnityVersion version)
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
		private float[] UpdateBoneArray(float[] array, UnityVersion version)
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

		private IReadOnlyList<Handle> GetExportHandles(UnityVersion version)
		{
			return HasHandles(version) ? Handles : System.Array.Empty<Handle>();
		}
		private IReadOnlyList<Collider> GetExportColliderArray(UnityVersion version)
		{
			return HasHandles(version) ? ColliderArray : System.Array.Empty<Collider>();
		}
		private IReadOnlyList<int> GetExportColliderIndex(UnityVersion version)
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

		public XForm RootX = new();
		public OffsetPtr<Skeleton> Skeleton = new();
		public OffsetPtr<SkeletonPose> SkeletonPose = new();
		public OffsetPtr<Hand> LeftHand = new();
		public OffsetPtr<Hand> RightHand = new();
	}
}
