using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Avatars
{
	public struct Human : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Less than 2018.2
		/// </summary>
		public static bool IsReadHandles(Version version)
		{
			return version.IsLess(2018, 2);
		}
		/// <summary>
		/// Less than 2018.2
		/// </summary>
		public static bool IsReadColliderIndex(Version version)
		{
			return version.IsLess(2018, 2);
		}
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool IsReadHasTDoF(Version version)
		{
			return version.IsGreaterEqual(5, 2);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5, 6, 1))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			RootX.Read(reader);
			Skeleton.Read(reader);
			SkeletonPose.Read(reader);
			LeftHand.Read(reader);
			RightHand.Read(reader);
			if(IsReadHandles(reader.Version))
			{
				m_handles = reader.ReadAssetArray<Handle>();
				m_colliderArray = reader.ReadAssetArray<Collider>();
			}

			int[] HumanBoneIndex = reader.ReadInt32Array();
			m_humanBoneIndex = UpdateBoneArray(HumanBoneIndex, reader.Version);
			float[] HumanBoneMass = reader.ReadSingleArray();
			m_humanBoneMass = UpdateBoneArray(HumanBoneMass, reader.Version);
			if (IsReadColliderIndex(reader.Version))
			{
				int[] ColliderIndex = reader.ReadInt32Array();
				m_colliderIndex = UpdateBoneArray(ColliderIndex, reader.Version);
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
			if (IsReadHasTDoF(reader.Version))
			{
				HasTDoF = reader.ReadBoolean();
			}
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
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
			return IsReadHandles(version) ? Handles : new Handle[0];
		}
		private IReadOnlyList<Collider> GetExportColliderArray(Version version)
		{
			return IsReadHandles(version) ? ColliderArray : new Collider[0];
		}
		private IReadOnlyList<int> GetExportColliderIndex(Version version)
		{
			return IsReadColliderIndex(version) ? ColliderIndex : new int[0];
		}

		public IReadOnlyList<Handle> Handles => m_handles;
		public IReadOnlyList<Collider> ColliderArray => m_colliderArray;
		public IReadOnlyList<int> HumanBoneIndex => m_humanBoneIndex;
		public IReadOnlyList<float> HumanBoneMass => m_humanBoneMass;
		public IReadOnlyList<int> ColliderIndex => m_colliderIndex;
		public float Scale { get; private set; }
		public float ArmTwist { get; private set; }
		public float ForeArmTwist { get; private set; }
		public float UpperLegTwist { get; private set; }
		public float LegTwist { get; private set; }
		public float ArmStretch { get; private set; }
		public float LegStretch { get; private set; }
		public float FeetSpacing { get; private set; }
		public bool HasLeftHand { get; private set; }
		public bool HasRightHand { get; private set; }
		public bool HasTDoF { get; private set; }

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
		
		private Handle[] m_handles;
		private Collider[] m_colliderArray;
		private int[] m_humanBoneIndex;
		private float[] m_humanBoneMass;
		private int[] m_colliderIndex;
	}
}
