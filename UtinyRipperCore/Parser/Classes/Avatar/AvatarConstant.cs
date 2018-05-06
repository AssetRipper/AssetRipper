using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Avatars
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

		public void Read(AssetStream stream)
		{
			AvatarSkeleton.Read(stream);
			AvatarSkeletonPose.Read(stream);
			if (IsReadDefaultPose(stream.Version))
			{
				DefaultPose.Read(stream);
				m_skeletonNameIDArray = stream.ReadUInt32Array();
			}
			Human.Read(stream);
			m_humanSkeletonIndexArray = stream.ReadInt32Array();
			if (IsReadHumanSkeletonReverseIndexArray(stream.Version))
			{
				m_humanSkeletonReverseIndexArray = stream.ReadInt32Array();
			}
			RootMotionBoneIndex = stream.ReadInt32();
			RootMotionBoneX.Read(stream);
			if (IsReadRootMotionSkeleton(stream.Version))
			{
				RootMotionSkeleton.Read(stream);
				RootMotionSkeletonPose.Read(stream);
				m_rootMotionSkeletonIndexArray = stream.ReadInt32Array();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_AvatarSkeleton", AvatarSkeleton.ExportYAML(container));
			node.Add("m_AvatarSkeletonPose", AvatarSkeletonPose.ExportYAML(container));
			node.Add("m_DefaultPose", DefaultPose.ExportYAML(container));
			node.Add("m_SkeletonNameIDArray", IsReadDefaultPose(container.Version) ? SkeletonNameIDArray.ExportYAML(true) : YAMLSequenceNode.Empty);
			node.Add("m_Human", Human.ExportYAML(container));
			node.Add("m_HumanSkeletonIndexArray", HumanSkeletonIndexArray.ExportYAML(true));
			node.Add("m_HumanSkeletonReverseIndexArray", IsReadHumanSkeletonReverseIndexArray(container.Version) ? HumanSkeletonReverseIndexArray.ExportYAML(true) : YAMLSequenceNode.Empty);
			node.Add("m_RootMotionBoneIndex", RootMotionBoneIndex);
			node.Add("m_RootMotionBoneX", RootMotionBoneX.ExportYAML(container));
			node.Add("m_RootMotionSkeleton", RootMotionSkeleton.ExportYAML(container));
			node.Add("m_RootMotionSkeletonPose", RootMotionSkeletonPose.ExportYAML(container));
			node.Add("m_RootMotionSkeletonIndexArray", IsReadRootMotionSkeleton(container.Version) ? RootMotionSkeletonIndexArray.ExportYAML(true) : YAMLSequenceNode.Empty);
			return node;
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
