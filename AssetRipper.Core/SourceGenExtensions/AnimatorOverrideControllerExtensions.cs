using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_221;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_93;
using AssetRipper.SourceGenerated.Subclasses.AnimationClipOverride;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AnimatorOverrideControllerExtensions
	{
		public static bool IsContainsAnimationClip(this IAnimatorOverrideController controller, IAnimationClip clip)
		{
			foreach (IAnimationClipOverride overClip in controller.Clips_C221)
			{
				if (overClip.OriginalClip.IsAsset(controller.Collection, clip))
				{
					return true;
				}
				else if (overClip.OverrideClip.IsAsset(controller.Collection, clip))
				{
					return true;
				}
			}
			IRuntimeAnimatorController? baseController = controller.Controller_C221P;
			if (baseController != null)
			{
				return baseController.IsContainsAnimationClip(clip);
			}
			return false;
		}
	}
}
