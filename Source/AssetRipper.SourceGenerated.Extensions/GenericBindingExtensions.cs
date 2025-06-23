using AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip;
using AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip.GenericBinding;
using AssetRipper.SourceGenerated.Subclasses.GenericBinding;

namespace AssetRipper.SourceGenerated.Extensions;

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
		return binding.Has_ClassID_UInt16() ? (ClassIDType)binding.ClassID_UInt16 : (ClassIDType)binding.ClassID_Int32;
	}

	public static void SetClassID(this IGenericBinding binding, ClassIDType classID)
	{
		if (binding.Has_ClassID_UInt16())
		{
			binding.ClassID_UInt16 = (ushort)classID;
		}
		else
		{
			binding.ClassID_Int32 = (int)classID;
		}
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
