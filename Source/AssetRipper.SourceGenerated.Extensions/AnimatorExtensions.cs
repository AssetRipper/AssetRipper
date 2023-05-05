using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Classes.ClassID_93;
using AssetRipper.SourceGenerated.Classes.ClassID_95;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class AnimatorExtensions
	{
		public static bool IsContainsAnimationClip(this IAnimator animator, IAnimationClip clip)
		{
			if (animator.Has_Controller_C95_PPtr_AnimatorController_4_0_0())
			{
				IAnimatorController? controller = animator.Controller_C95_PPtr_AnimatorController_4_0_0P;
				return controller is not null && controller.IsContainsAnimationClip(clip);
			}
			else if (animator.Has_Controller_C95_PPtr_RuntimeAnimatorController_4_3_0())
			{
				IRuntimeAnimatorController? controller = animator.Controller_C95_PPtr_RuntimeAnimatorController_4_3_0P;
				return controller is not null && controller.IsContainsAnimationClip(clip);
			}
			else if (animator.Has_Controller_C95_PPtr_RuntimeAnimatorController_5_0_0())
			{
				IRuntimeAnimatorController? controller = animator.Controller_C95_PPtr_RuntimeAnimatorController_5_0_0P;
				return controller is not null && controller.IsContainsAnimationClip(clip);
			}
			else
			{
				return false;
			}
		}

		public static IReadOnlyDictionary<uint, string> BuildTOS(this IAnimator animator)
		{
			if (animator.Has_HasTransformHierarchy_C95() && !animator.HasTransformHierarchy_C95)
			{
				return new Dictionary<uint, string>() { { 0, string.Empty } };
			}

			return animator.GameObject_C95.GetAsset(animator.Collection).BuildTOS();
		}

		public static AnimatorUpdateMode GetUpdateMode(this IAnimator animator)
		{
			return animator.Has_UpdateMode_C95() ? animator.UpdateMode_C95E : AnimatorUpdateMode.Normal;
		}
	}
}
