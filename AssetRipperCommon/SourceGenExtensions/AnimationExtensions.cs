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
	}
}
