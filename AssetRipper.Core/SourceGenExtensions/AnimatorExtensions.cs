using AssetRipper.Core.Classes.Animator;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Classes.ClassID_93;
using AssetRipper.SourceGenerated.Classes.ClassID_95;
using System.Collections.Generic;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AnimatorExtensions
	{
		public static bool IsContainsAnimationClip(this IAnimator animator, IAnimationClip clip)
		{
			if (animator.Has_Controller_C95_PPtr_AnimatorController__4_0_0_f7())
			{
				IAnimatorController? controller = animator.Controller_C95_PPtr_AnimatorController__4_0_0_f7P;
				return controller is not null && controller.IsContainsAnimationClip(clip);
			}
			else if (animator.Has_Controller_C95_PPtr_RuntimeAnimatorController__4_3_0_f4())
			{
				IRuntimeAnimatorController? controller = animator.Controller_C95_PPtr_RuntimeAnimatorController__4_3_0_f4P;
				return controller is not null && controller.IsContainsAnimationClip(clip);
			}
			else if (animator.Has_Controller_C95_PPtr_RuntimeAnimatorController__5_0_0_f4())
			{
				IRuntimeAnimatorController? controller = animator.Controller_C95_PPtr_RuntimeAnimatorController__5_0_0_f4P;
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
					IGameObject go = animator.GameObject_C95.GetAsset(animator.SerializedFile);
					return go.BuildTOS();
				}
				else
				{
					return new Dictionary<uint, string>() { { 0, string.Empty } };
				}
			}
			else
			{
				IGameObject go = animator.GameObject_C95.GetAsset(animator.SerializedFile);
				return go.BuildTOS();
			}
		}

		public static AnimatorCullingMode GetCullingMode(this IAnimator animator)
		{
			return (AnimatorCullingMode)animator.CullingMode_C95;
		}

		public static AnimatorUpdateMode GetUpdateMode(this IAnimator animator)
		{
			return animator.Has_UpdateMode_C95() ? (AnimatorUpdateMode)animator.UpdateMode_C95 : AnimatorUpdateMode.Normal;
		}
	}
}
