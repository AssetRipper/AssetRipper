using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Classes.ClassID_93;
using AssetRipper.SourceGenerated.Classes.ClassID_95;
using AssetRipper.SourceGenerated.Enums;
using System.Collections.Generic;

namespace AssetRipper.Core.SourceGenExtensions
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
			if (animator.Has_HasTransformHierarchy_C95())
			{
				if (animator.HasTransformHierarchy_C95)
				{
					IGameObject go = animator.GameObject_C95.GetAsset(animator.Collection);
					return go.BuildTOS();
				}
				else
				{
					return new Dictionary<uint, string>() { { 0, string.Empty } };
				}
			}
			else
			{
				IGameObject go = animator.GameObject_C95.GetAsset(animator.Collection);
				return go.BuildTOS();
			}
		}

		public static AnimatorUpdateMode GetUpdateMode(this IAnimator animator)
		{
			return animator.Has_UpdateMode_C95() ? animator.UpdateMode_C95E : AnimatorUpdateMode.Normal;
		}
	}
}
