using AssetRipper.SourceGenerated.Classes.ClassID_221;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Classes.ClassID_93;

namespace AssetRipper.SourceGenerated.Extensions;

public static class RuntimeAnimatorControllerExtensions
{
	public static bool ContainsAnimationClip(this IRuntimeAnimatorController controller, IAnimationClip clip)
	{
		return controller switch
		{
			IAnimatorController animatorController => animatorController.ContainsAnimationClip(clip),
			IAnimatorOverrideController overrideController => overrideController.ContainsAnimationClip(clip),
			_ => throw new Exception(GetExceptionMessage(controller))
		};

		static string GetExceptionMessage(IRuntimeAnimatorController controller)
		{
			return $"{controller.GetType()} inherits from {nameof(IRuntimeAnimatorController)} but not {nameof(IAnimatorController)} or {nameof(IAnimatorOverrideController)}";
		}
	}
}
