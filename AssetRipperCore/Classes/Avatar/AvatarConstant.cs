using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Avatar
{
	public sealed class AvatarConstant : IAssetReadable, IYAMLExportable
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreater(4, 3))
			{
				return 3;
			}
			// unknown (alpha) version
			//return 2;

			return 1;
		}

		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasDefaultPose(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasHumanSkeletonReverseIndexArray(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasRootMotionSkeleton(UnityVersion version) => version.IsGreaterEqual(4, 3);

		public void Read(AssetReader reader)
		{
			AvatarSkeleton.Read(reader);
			AvatarSkeletonPose.Read(reader);
			if (HasDefaultPose(reader.Version))
			{
				DefaultPose.Read(reader);
				SkeletonNameIDArray = reader.ReadUInt32Array();
			}
			Human.Read(reader);
			HumanSkeletonIndexArray = reader.ReadInt32Array();
			if (HasHumanSkeletonReverseIndexArray(reader.Version))
			{
				HumanSkeletonReverseIndexArray = reader.ReadInt32Array();
			}
			else
			{
				HumanSkeletonReverseIndexArray = new int[AvatarSkeleton.Instance.Node.Length];
				for (int i = 0; i < AvatarSkeleton.Instance.Node.Length; i++)
				{
					HumanSkeletonReverseIndexArray[i] = HumanSkeletonIndexArray.IndexOf(i);
				}
			}
			RootMotionBoneIndex = reader.ReadInt32();
			RootMotionBoneX.Read(reader);
			if (HasRootMotionSkeleton(reader.Version))
			{
				RootMotionSkeleton.Read(reader);
				RootMotionSkeletonPose.Read(reader);
				RootMotionSkeletonIndexArray = reader.ReadInt32Array();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(AvatarSkeletonName, AvatarSkeleton.ExportYAML(container));
			node.Add(AvatarSkeletonPoseName, AvatarSkeletonPose.ExportYAML(container));
			node.Add(DefaultPoseName, GetDefaultPose(container.Version).ExportYAML(container));
			node.Add(SkeletonNameIDArrayName, GetSkeletonNameIDArray(container.Version).ExportYAML(true));
			node.Add(HumanName, Human.ExportYAML(container));
			node.Add(HumanSkeletonIndexArrayName, HumanSkeletonIndexArray.ExportYAML(true));
			node.Add(HumanSkeletonReverseIndexArrayName, HumanSkeletonReverseIndexArray.ExportYAML(true));
			node.Add(RootMotionBoneIndexName, RootMotionBoneIndex);
			node.Add(RootMotionBoneXName, RootMotionBoneX.ExportYAML(container));
			node.Add(RootMotionSkeletonName, RootMotionSkeleton.ExportYAML(container));
			node.Add(RootMotionSkeletonPoseName, RootMotionSkeletonPose.ExportYAML(container));
			node.Add(RootMotionSkeletonIndexArrayName, GetRootMotionSkeletonIndexArray(container.Version).ExportYAML(true));
			return node;
		}

		private OffsetPtr<SkeletonPose> GetDefaultPose(UnityVersion version)
		{
			return HasDefaultPose(version) ? DefaultPose : AvatarSkeletonPose;
		}
		private IReadOnlyList<uint> GetSkeletonNameIDArray(UnityVersion version)
		{
			return HasDefaultPose(version) ? SkeletonNameIDArray : AvatarSkeleton.Instance.ID;
		}
		private IReadOnlyList<int> GetRootMotionSkeletonIndexArray(UnityVersion version)
		{
			return HasRootMotionSkeleton(version) ? RootMotionSkeletonIndexArray : System.Array.Empty<int>();
		}

		public uint[] SkeletonNameIDArray { get; set; }
		public int[] HumanSkeletonIndexArray { get; set; }
		public int[] HumanSkeletonReverseIndexArray { get; set; }
		public int RootMotionBoneIndex { get; set; }
		public int[] RootMotionSkeletonIndexArray { get; set; }

		public const string AvatarSkeletonName = "m_AvatarSkeleton";
		public const string AvatarSkeletonPoseName = "m_AvatarSkeletonPose";
		public const string DefaultPoseName = "m_DefaultPose";
		public const string SkeletonNameIDArrayName = "m_SkeletonNameIDArray";
		public const string HumanName = "m_Human";
		public const string HumanSkeletonIndexArrayName = "m_HumanSkeletonIndexArray";
		public const string HumanSkeletonReverseIndexArrayName = "m_HumanSkeletonReverseIndexArray";
		public const string RootMotionBoneIndexName = "m_RootMotionBoneIndex";
		public const string RootMotionBoneXName = "m_RootMotionBoneX";
		public const string RootMotionSkeletonName = "m_RootMotionSkeleton";
		public const string RootMotionSkeletonPoseName = "m_RootMotionSkeletonPose";
		public const string RootMotionSkeletonIndexArrayName = "m_RootMotionSkeletonIndexArray";

		/// <summary>
		/// Skeleton previously
		/// </summary>
		public OffsetPtr<Skeleton> AvatarSkeleton = new();
		/// <summary>
		/// SkeletonPose previously
		/// </summary>
		public OffsetPtr<SkeletonPose> AvatarSkeletonPose = new();
		public OffsetPtr<SkeletonPose> DefaultPose = new();
		public OffsetPtr<Human> Human = new();
		public XForm RootMotionBoneX = new();
		public OffsetPtr<Skeleton> RootMotionSkeleton = new();
		public OffsetPtr<SkeletonPose> RootMotionSkeletonPose = new();
	}
}
