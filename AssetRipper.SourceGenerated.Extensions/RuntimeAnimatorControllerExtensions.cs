using AssetRipper.SourceGenerated.Classes.ClassID_221;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Classes.ClassID_93;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class RuntimeAnimatorControllerExtensions
	{
		public static bool IsContainsAnimationClip(this IRuntimeAnimatorController controller, IAnimationClip clip)
		{
			if (controller is IAnimatorController animatorController)
			{
				return animatorController.IsContainsAnimationClip(clip);
			}
			else if (controller is IAnimatorOverrideController overrideController)
			{
				return overrideController.IsContainsAnimationClip(clip);
			}
			else
			{
				throw new Exception($"{controller.GetType()} inherits from {nameof(IRuntimeAnimatorController)} but not {nameof(IAnimatorController)} or {nameof(IAnimatorOverrideController)}");
			}
		}
	}
}
