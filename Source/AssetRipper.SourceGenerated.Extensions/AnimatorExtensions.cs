using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Classes.ClassID_93;
using AssetRipper.SourceGenerated.Classes.ClassID_95;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class AnimatorExtensions
	{
		public static bool ContainsAnimationClip(this IAnimator animator, IAnimationClip clip)
		{
			if (animator.Has_Controller_PPtr_AnimatorController_4_0_0())
			{
				IAnimatorController? controller = animator.Controller_PPtr_AnimatorController_4_0_0P;
				return controller is not null && controller.ContainsAnimationClip(clip);
			}
			else if (animator.Has_Controller_PPtr_RuntimeAnimatorController_4_3_0())
			{
				IRuntimeAnimatorController? controller = animator.Controller_PPtr_RuntimeAnimatorController_4_3_0P;
				return controller is not null && controller.ContainsAnimationClip(clip);
			}
			else if (animator.Has_Controller_PPtr_RuntimeAnimatorController_5_0_0())
			{
				IRuntimeAnimatorController? controller = animator.Controller_PPtr_RuntimeAnimatorController_5_0_0P;
				return controller is not null && controller.ContainsAnimationClip(clip);
			}
			else
			{
				return false;
			}
		}

		public static AnimatorUpdateMode GetUpdateMode(this IAnimator animator)
		{
			return animator.Has_UpdateMode() ? animator.UpdateModeE : AnimatorUpdateMode.Normal;
		}
	}
}
