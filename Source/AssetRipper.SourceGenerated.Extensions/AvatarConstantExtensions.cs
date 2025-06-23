using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Subclasses.AvatarConstant;
using AssetRipper.SourceGenerated.Subclasses.OffsetPtr_SkeletonPose;

namespace AssetRipper.SourceGenerated.Extensions;

public static class AvatarConstantExtensions
{
	public static IOffsetPtr_SkeletonPose GetDefaultPose(this IAvatarConstant constant)
	{
		return constant.Has_DefaultPose() ? constant.DefaultPose : constant.SkeletonPose;
	}

	public static AssetList<uint> GetSkeletonNameIDArray(this IAvatarConstant constant)
	{
		return constant.Has_SkeletonNameIDArray() ? constant.SkeletonNameIDArray : constant.Skeleton.Data.ID;
	}
}
