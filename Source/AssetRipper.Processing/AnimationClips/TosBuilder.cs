﻿using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Utils;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_111;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_90;
using AssetRipper.SourceGenerated.Classes.ClassID_95;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.GenericBinding;

namespace AssetRipper.Processing.AnimationClips;

/// <summary>
/// A CRC32 lookup generator.
/// </summary>
/// <remarks>
/// TOS comes from <see cref="IAvatar.TOS_C90"/>. It presumably means "Table of Strings," but there doesn't seem to be any documentation on it.
/// </remarks>
internal static class TosBuilder
{
	private static IReadOnlyDictionary<uint, string> BuildTOS(this IAnimator animator)
	{
		if (animator.Has_HasTransformHierarchy_C95() && !animator.HasTransformHierarchy_C95)
		{
			return MakeNewDictionary();
		}

		return animator.GameObject_C95.GetAsset(animator.Collection).BuildTOS();
	}

	public static IReadOnlyDictionary<uint, string> FindTOS(this IAnimationClip clip, AnimationCache cache)
	{
		//
		// We don't stop adding
		// after a match is found.
		//
		// This may be more performance intensive.
		// However, it ensures a more precise
		// recovery of all TOS data.
		//
		// Sometimes TOS data that is needed isnt
		// in the specified avatar, animator, or animation even
		// if it contains the specified animationclip.
		//
		// By adding all of them, we reduce
		// the potential of missing anything.
		//

		Dictionary<uint, string> tos = MakeNewDictionary();

		Span<IAvatar> avatars = cache.CachedAvatars.AsSpan();
		Span<IAnimator> animators = cache.CachedAnimators.AsSpan();
		Span<IAnimation> animations = cache.CachedAnimations.AsSpan();

		int avatarsCount = cache.CachedAvatars.Length;
		int animatorsCount = cache.CachedAnimators.Length;
		int animationsCount = cache.CachedAnimations.Length;

		int maxCount = Math.Max(Math.Max(avatarsCount, animatorsCount), animationsCount);

		for (int i = 0; i < maxCount; i++)
		{
			if (i < avatarsCount)
			{
				clip.AddAvatarTOS(avatars[i], tos);
			}

			if (i < animatorsCount)
			{
				clip.AddAnimatorTOS(animators[i], tos);
			}

			if (i < animationsCount)
			{
				clip.AddAnimationTOS(animations[i], tos);
			}
		}

		return tos;
	}

	private static Dictionary<uint, string> MakeNewDictionary()
	{
		return new() { { 0, string.Empty } };
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
		if (!clip.Has_ClipBindingConstant_C74())
		{
			return false;
		}

		AccessListBase<IGenericBinding> bindings = clip.ClipBindingConstant_C74.GenericBindings;

		bool allFound = true;

		for (int i = 0; i < bindings.Count; i++)
		{
			uint bindingPath = bindings[i].Path;

			if (src.TryGetValue(bindingPath, out string? path))
			{
				dest[bindingPath] = path;
			}
			else if (bindingPath != 0)
			{
				allFound = false;
			}
		}

		return allFound;
	}

	private static bool AddTOS(this IAnimationClip clip, AssetDictionary<uint, Utf8String> src, Dictionary<uint, string> dest)
	{
		if (!clip.Has_ClipBindingConstant_C74())
		{
			return false;
		}

		AccessListBase<IGenericBinding> bindings = clip.ClipBindingConstant_C74.GenericBindings;
		bool allFound = true;

		for (int i = 0; i < bindings.Count; i++)
		{
			uint bindingPath = bindings[i].Path;

			if (src.TryGetValue(bindingPath, out Utf8String? path))
			{
				dest[bindingPath] = path;
			}
			else if (bindingPath != 0)
			{
				allFound = false;
			}
		}

		return allFound;
	}

	private static IReadOnlyDictionary<uint, string> BuildTOS(this IGameObject gameObject)
	{
		Dictionary<uint, string> tos = MakeNewDictionary();
		gameObject.BuildTOS(gameObject, string.Empty, tos);
		return tos;
	}

	private static void BuildTOS(this IGameObject gameObject, IGameObject parent, string parentPath, Dictionary<uint, string> tos)
	{
		ITransform transform = parent.GetTransform();

		foreach (ITransform? childTransform in transform.Children_C4P)
		{
			IGameObject child = childTransform?.GameObject_C4P ?? throw new NullReferenceException();

			string path = string.IsNullOrEmpty(parentPath)
				? child.NameString
				: $"{parentPath}/{child.NameString}";

			uint pathHash = CrcUtils.CalculateDigestUTF8(path);
			tos[pathHash] = path;

			gameObject.BuildTOS(child, path, tos);
		}
	}
}
