using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_111;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_90;
using AssetRipper.SourceGenerated.Classes.ClassID_95;
using AssetRipper.SourceGenerated.Subclasses.GenericBinding;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;
using System.Collections.Generic;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AnimationClipExtensions
	{
		private static List<IAvatar> _cachedAvatars = new();
		private static List<IAnimator> _cachedAnimators = new();
		private static List<IAnimation> _cachedAnimations = new();
		
		public enum AnimationType
		{
			Legacy = 1,
			Mecanim = 2,
			Human = 3,
		}

		public static bool GetLegacy(this IAnimationClip clip)
		{
			if (clip.Has_Legacy_C74())
			{
				return clip.Legacy_C74;
			}
			return clip.AnimationType_C74 == (int)AnimationType.Legacy;
		}

		public static void ClearCachedAssets()
		{
			_cachedAnimations.Clear();
			_cachedAnimators.Clear();
			_cachedAvatars.Clear();
		}
		
		public static void CacheAssets(Bundle bundle)
		{
			ClearCachedAssets();
			
			foreach (IUnityObjectBase asset in bundle.FetchAssetsInHierarchy())
			{
				switch (asset)
				{
					case IAvatar avatar:
						_cachedAvatars.Add(avatar);
						break;
					case IAnimator animator:
						_cachedAnimators.Add(animator);
						break;
					case IAnimation animation:
						_cachedAnimations.Add(animation);
						break;
				}
			}
		}

		public static IEnumerable<IGameObject> FindRoots(this IAnimationClip clip)
		{
			foreach (IUnityObjectBase asset in clip.Collection.Bundle.FetchAssetsInHierarchy())
			{
				if (asset is IAnimator animator)
				{
					if (clip.IsAnimatorContainsClip(animator))
					{
						yield return animator.GameObject_C8.GetAsset(animator.Collection);
					}
				}
				else if (asset is IAnimation animation)
				{
					if (clip.IsAnimationContainsClip(animation))
					{
						yield return animation.GameObject_C8.GetAsset(animation.Collection);
					}
				}
			}

			yield break;
		}

		private static bool IsAnimatorContainsClip(this IAnimationClip clip, IAnimator animator)
		{
			return animator.IsContainsAnimationClip(clip);
		}

		private static bool IsAnimationContainsClip(this IAnimationClip clip, IAnimation animation)
		{
			return animation.IsContainsAnimationClip(clip);
		}

		public static IReadOnlyDictionary<uint, string> FindTOS(this IAnimationClip clip)
		{
			Dictionary<uint, string> tos = new()
			{
				{ 0, string.Empty }
			};
			
			foreach (IAvatar avatar in _cachedAvatars)
			{
				if (clip.AddAvatarTOS(avatar, tos))
				{
					return tos;
				}
			}
			
			foreach (IAnimator animator in _cachedAnimators)
			{
				if (clip.IsAnimatorContainsClip(animator) && clip.AddAnimatorTOS(animator, tos))
				{
					return tos;
				}
			}
			
			foreach (IAnimation animation in _cachedAnimations)
			{
				if (clip.IsAnimationContainsClip(animation) && clip.AddAnimationTOS(animation, tos))
				{
					return tos;
				}
			}

			return tos;
		}

		private static bool AddAvatarTOS(this IAnimationClip clip, IAvatar avatar, Dictionary<uint, string> tos)
		{
			return clip.AddTOS(avatar.TOS_C90, tos);
		}

		private static bool AddAnimatorTOS(this IAnimationClip clip, IAnimator animator, Dictionary<uint, string> tos)
		{
			IAvatar? avatar = animator.Avatar_C95P;
			if (avatar != null && clip.AddAvatarTOS(avatar, tos))
			{
				return true;
			}

			IReadOnlyDictionary<uint, string> animatorTOS = animator.BuildTOS();
			return clip.AddTOS(animatorTOS, tos);
		}

		private static bool AddAnimationTOS(this IAnimationClip clip, IAnimation animation, Dictionary<uint, string> tos)
		{
			IGameObject go = animation.GameObject_C8.GetAsset(animation.Collection);
			IReadOnlyDictionary<uint, string> animationTOS = go.BuildTOS();
			return clip.AddTOS(animationTOS, tos);
		}

		private static bool AddTOS(this IAnimationClip clip, IReadOnlyDictionary<uint, string> src, Dictionary<uint, string> dest)
		{
			if (clip.Has_ClipBindingConstant_C74())
			{
				int tosCount = clip.ClipBindingConstant_C74.GenericBindings.Count;
				for (int i = 0; i < tosCount; i++)
				{
					IGenericBinding binding = clip.ClipBindingConstant_C74.GenericBindings[i];
					if (src.TryGetValue(binding.Path, out string? path))
					{
						dest[binding.Path] = path;
						if (dest.Count == tosCount)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		private static bool AddTOS(this IAnimationClip clip, AssetDictionary<uint, Utf8String> src, Dictionary<uint, string> dest)
		{
			if (clip.Has_ClipBindingConstant_C74())
			{
				int tosCount = clip.ClipBindingConstant_C74.GenericBindings.Count;
				for (int i = 0; i < tosCount; i++)
				{
					IGenericBinding binding = clip.ClipBindingConstant_C74.GenericBindings[i];
					if (src.TryGetValue(binding.Path, out Utf8String? path))
					{
						dest[binding.Path] = path.String;
						if (dest.Count == tosCount)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
