﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.SourceGenerated.Classes.ClassID_221;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_93;
using AssetRipper.SourceGenerated.Subclasses.AnimationClipOverride;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AnimatorOverrideControllerExtensions
	{
		public static bool IsContainsAnimationClip(this IAnimatorOverrideController controller, IAnimationClip clip)
		{
			foreach (IAnimationClipOverride overClip in controller.Clips_C221)
			{
				if (overClip.OriginalClip.IsAsset(controller.SerializedFile, clip))
				{
					return true;
				}
				else if (overClip.OverrideClip.IsAsset(controller.SerializedFile, clip))
				{
					return true;
				}
			}
			IRuntimeAnimatorController? baseController = controller.Controller_C221.FindAsset(controller.SerializedFile);
			if (baseController != null)
			{
				return baseController.IsContainsAnimationClip(clip);
			}
			return false;
		}
	}
}
