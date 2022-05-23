﻿using AssetRipper.Core.Classes.AnimationClip;
using AssetRipper.Core.Classes.AnimationClip.GenericBinding;
using AssetRipper.SourceGenerated.Subclasses.GenericBinding;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class GenericBindingExtensions
	{
		public static HumanoidMuscleType GetHumanoidMuscle(this IGenericBinding binding, UnityVersion version)
		{
			return ((HumanoidMuscleType)binding.Attribute).Update(version);
		}

		public static bool IsTransform(this IGenericBinding binding)
		{
			return binding.GetClassID() == ClassIDType.Transform 
				|| binding.GetClassID() == ClassIDType.RectTransform 
				&& binding.TransformType().IsValid();
		}

		public static TransformType TransformType(this IGenericBinding binding)
		{
			return unchecked((TransformType)binding.Attribute);
		}

		public static ClassIDType GetClassID(this IGenericBinding binding)
		{
			return binding.Has_ClassID() ? (ClassIDType)binding.ClassID : (ClassIDType)binding.TypeID;
		}

		public static bool IsPPtrCurve(this IGenericBinding binding)
		{
			return binding.IsPPtrCurve != 0;
		}

		public static bool IsIntCurve(this IGenericBinding binding)
		{
			return binding.IsIntCurve != 0;
		}
	}
}
