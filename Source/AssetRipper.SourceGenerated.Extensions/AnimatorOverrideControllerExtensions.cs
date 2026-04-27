using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_221;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Classes.ClassID_93;
using AssetRipper.SourceGenerated.Subclasses.AnimationClipOverride;

namespace AssetRipper.SourceGenerated.Extensions;

public static class AnimatorOverrideControllerExtensions
{
	public static bool ContainsAnimationClip(this IAnimatorOverrideController controller, IAnimationClip clip)
	{
		foreach (IAnimationClipOverride overClip in controller.Clips)
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
		IRuntimeAnimatorController? baseController = controller.ControllerP;
		if (baseController != null)
		{
			return baseController.ContainsAnimationClip(clip);
		}
		return false;
	}

	public static IEnumerable<IUnityObjectBase?> FetchEditorHierarchy(this IAnimatorOverrideController animatorOverrideController)
	{
		yield return animatorOverrideController;

		IRuntimeAnimatorController? baseController = animatorOverrideController.ControllerP;
		if (baseController is IAnimatorController animatorController)
		{
			foreach (IUnityObjectBase? asset in animatorController.FetchEditorHierarchy())
			{
				yield return asset;
			}
		}

		foreach (IAnimationClipOverride overClip in animatorOverrideController.Clips)
		{
			yield return overClip.OverrideClip.TryGetAsset(animatorOverrideController.Collection);
		}
	}
}
