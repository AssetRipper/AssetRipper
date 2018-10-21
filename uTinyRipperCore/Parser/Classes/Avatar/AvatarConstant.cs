using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.Avatars
{
	public struct AvatarConstant : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadDefaultPose(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadHumanSkeletonReverseIndexArray(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadRootMotionSkeleton(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		
		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 3;
			}
			
#warning unknown
			if (version.IsGreaterEqual(4, 3, 0, VersionType.Beta, 1))
			{
				return 3;
			}
			if (version.IsGreater(4, 3))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			AvatarSkeleton.Read(reader);
			AvatarSkeletonPose.Read(reader);
			if (IsReadDefaultPose(reader.Version))
			{
				DefaultPose.Read(reader);
				m_skeletonNameIDArray = reader.ReadUInt32Array();
			}
			Human.Read(reader);
			m_humanSkeletonIndexArray = reader.ReadInt32Array();
			if (IsReadHumanSkeletonReverseIndexArray(reader.Version))
			{
				m_humanSkeletonReverseIndexArray = reader.ReadInt32Array();
			}
			else
			{
				m_humanSkeletonReverseIndexArray = new int[AvatarSkeleton.Instance.Node.Count];
				for (int i = 0; i < AvatarSkeleton.Instance.Node.Count; i++)
				{
					m_humanSkeletonReverseIndexArray[i] = m_humanSkeletonIndexArray.IndexOf(i);
				}
			}
			RootMotionBoneIndex = reader.ReadInt32();
			RootMotionBoneX.Read(reader);
			if (IsReadRootMotionSkeleton(reader.Version))
			{
				RootMotionSkeleton.Read(reader);
				RootMotionSkeletonPose.Read(reader);
				m_rootMotionSkeletonIndexArray = reader.ReadInt32Array();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_AvatarSkeleton", AvatarSkeleton.ExportYAML(container));
			node.Add("m_AvatarSkeletonPose", AvatarSkeletonPose.ExportYAML(container));
			node.Add("m_DefaultPose", GetDefaultPose(container.Version).ExportYAML(container));
			node.Add("m_SkeletonNameIDArray", GetSkeletonNameIDArray(container.Version).ExportYAML(true));
			node.Add("m_Human", Human.ExportYAML(container));
			node.Add("m_HumanSkeletonIndexArray", HumanSkeletonIndexArray.ExportYAML(true));
			node.Add("m_HumanSkeletonReverseIndexArray", HumanSkeletonReverseIndexArray.ExportYAML(true));
			node.Add("m_RootMotionBoneIndex", RootMotionBoneIndex);
			node.Add("m_RootMotionBoneX", RootMotionBoneX.ExportYAML(container));
			node.Add("m_RootMotionSkeleton", RootMotionSkeleton.ExportYAML(container));
			node.Add("m_RootMotionSkeletonPose", RootMotionSkeletonPose.ExportYAML(container));
			node.Add("m_RootMotionSkeletonIndexArray", GetRootMotionSkeletonIndexArray(container.Version).ExportYAML(true));
			return node;
		}

		private OffsetPtr<SkeletonPose> GetDefaultPose(Version version)
		{
			return IsReadDefaultPose(version) ? DefaultPose : AvatarSkeletonPose;
		}
		private IReadOnlyList<uint> GetSkeletonNameIDArray(Version version)
		{
			return IsReadDefaultPose(version) ? SkeletonNameIDArray : AvatarSkeleton.Instance.ID;
		}
		private IReadOnlyList<int> GetRootMotionSkeletonIndexArray(Version version)
		{
			return IsReadRootMotionSkeleton(version) ? RootMotionSkeletonIndexArray : new int[0];
		}

		public IReadOnlyList<uint> SkeletonNameIDArray => m_skeletonNameIDArray;
		public IReadOnlyList<int> HumanSkeletonIndexArray => m_humanSkeletonIndexArray;
		public IReadOnlyList<int> HumanSkeletonReverseIndexArray => m_humanSkeletonReverseIndexArray;
		public int RootMotionBoneIndex { get; private set; }
		public IReadOnlyList<int> RootMotionSkeletonIndexArray => m_rootMotionSkeletonIndexArray;

		/// <summary>
		/// Skeleton previously
		/// </summary>
		public OffsetPtr<Skeleton> AvatarSkeleton;
		/// <summary>
		/// SkeletonPose previously
		/// </summary>
		public OffsetPtr<SkeletonPose> AvatarSkeletonPose;
		public OffsetPtr<SkeletonPose> DefaultPose;
		public OffsetPtr<Human> Human;
		public XForm RootMotionBoneX;
		public OffsetPtr<Skeleton> RootMotionSkeleton;
		public OffsetPtr<SkeletonPose> RootMotionSkeletonPose;
		
		private uint[] m_skeletonNameIDArray;
		private int[] m_humanSkeletonIndexArray;
		private int[] m_humanSkeletonReverseIndexArray;
		private int[] m_rootMotionSkeletonIndexArray;
	}
}
