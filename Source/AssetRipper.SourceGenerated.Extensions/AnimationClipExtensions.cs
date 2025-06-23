using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_111;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_95;
using AssetRipper.SourceGenerated.NativeEnums.Global;

namespace AssetRipper.SourceGenerated.Extensions;

public static class AnimationClipExtensions
{
	public static bool GetLegacy(this IAnimationClip clip)
	{
		if (clip.Has_Legacy_C74())
		{
			return clip.Legacy_C74;
		}
		return clip.AnimationType_C74 == (int)AnimationType.Legacy;
	}

	public static IEnumerable<IGameObject> FindRoots(this IAnimationClip clip)
	{
		foreach (IUnityObjectBase asset in clip.Collection.Bundle.FetchAssetsInHierarchy())
		{
			if (asset is IAnimator animator)
			{
				if (animator.ContainsAnimationClip(clip))
				{
					IGameObject? gameObject = animator.GameObjectP;
					if (gameObject is not null)
					{
						yield return gameObject;
					}
				}
			}
			else if (asset is IAnimation animation)
			{
				if (animation.ContainsAnimationClip(clip))
				{
					IGameObject? gameObject = animation.GameObjectP;
					if (gameObject is not null)
					{
						yield return gameObject;
					}
				}
			}
		}

		yield break;
	}

	public static bool SupportsNegativeInfinitySlopes(this IAnimationClip clip) => clip.Collection.Version.GreaterThan(2021);
}
