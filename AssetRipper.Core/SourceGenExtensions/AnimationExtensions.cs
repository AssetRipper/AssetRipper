using AssetRipper.Core.Classes.Animation;
using AssetRipper.Core.Classes.AnimationClip;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.SourceGenerated.Classes.ClassID_111;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimationClip_;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AnimationExtensions
	{
		public static bool IsContainsAnimationClip(this IAnimation animation, IAnimationClip clip)
		{
			foreach (IPPtr_AnimationClip_ clipPtr in animation.Animations_C111)
			{
				if (clipPtr.IsAsset(animation.SerializedFile, clip))
				{
					return true;
				}
			}
			return false;
		}

		public static WrapMode GetWrapMode(this IAnimation animation)
		{
			return (WrapMode)animation.WrapMode_C111;
		}

		public static bool GetAnimateOnlyIfVisible(this IAnimation animation)
		{
			// 2.6.0 to 3.4.0 exclusive
			if (animation.Has_AnimateOnlyIfVisible_C111())
			{
				return animation.AnimateOnlyIfVisible_C111;
			}
			//else if (animation.Has_CullingType_C111())
			//{
			return (AnimationCullingType)animation.CullingType_C111 != AnimationCullingType.AlwaysAnimate;
			//}
			//else
			//{
			//	return false;
			//}
		}

		public static AnimationCullingType GetAnimationCullingType(this IAnimation animation)
		{
			// 2.6.0 to 3.4.0 exclusive
			if (animation.Has_AnimateOnlyIfVisible_C111())
			{
				return animation.AnimateOnlyIfVisible_C111 ? AnimationCullingType.BasedOnRenderers : AnimationCullingType.AlwaysAnimate;
			}
			//else if (animation.Has_CullingType_C111())
			//{
			return (AnimationCullingType)animation.CullingType_C111;
			//}
			//else
			//{
			//	return default;
			//}
		}
	}
}
