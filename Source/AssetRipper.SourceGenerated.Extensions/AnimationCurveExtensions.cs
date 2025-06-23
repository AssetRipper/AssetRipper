using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.AnimationCurve_Quaternionf;
using AssetRipper.SourceGenerated.Subclasses.AnimationCurve_Single;
using AssetRipper.SourceGenerated.Subclasses.AnimationCurve_Vector3f;
using AssetRipper.SourceGenerated.Subclasses.Keyframe_Single;

namespace AssetRipper.SourceGenerated.Extensions;

public static class AnimationCurveExtensions
{
	public static void SetDefaultRotationOrderAndCurveLoopType(this IAnimationCurve_Single animationCurve)
	{
		animationCurve.PreInfinityE = CurveLoopTypes.CycleWithOffset;
		animationCurve.PostInfinityE = CurveLoopTypes.CycleWithOffset;
		animationCurve.RotationOrderE = RotationOrder.OrderZXY;
	}

	public static void SetValues(this IAnimationCurve_Single animationCurve, UnityVersion version, float defaultValue, float defaultWeight)
	{
		animationCurve.SetDefaultRotationOrderAndCurveLoopType();
		animationCurve.Curve.Capacity = 2;
		animationCurve.Curve.AddNew().SetValues(version, 0.0f, defaultValue, defaultWeight);
		animationCurve.Curve.AddNew().SetValues(version, 1.0f, defaultValue, defaultWeight);
	}

	public static void SetValues(this IAnimationCurve_Single animationCurve, UnityVersion version, float value1, float value2, float defaultWeight)
	{
		animationCurve.SetDefaultRotationOrderAndCurveLoopType();
		animationCurve.Curve.Capacity = 2;
		animationCurve.Curve.AddNew().SetValues(version, 0.0f, value1, defaultWeight);
		animationCurve.Curve.AddNew().SetValues(version, 1.0f, value2, defaultWeight);
	}

	public static void SetValues(this IAnimationCurve_Single animationCurve, UnityVersion version, float value1, float inSlope1, float outSlope1, float value2, float inSlope2, float outSlope2, float defaultWeight)
	{
		animationCurve.SetDefaultRotationOrderAndCurveLoopType();
		animationCurve.Curve.Capacity = 2;
		animationCurve.Curve.AddNew().SetValues(version, 0.0f, value1, inSlope1, outSlope1, defaultWeight);
		animationCurve.Curve.AddNew().SetValues(version, 1.0f, value2, inSlope2, outSlope2, defaultWeight);
	}

	public static void SetValues(this IAnimationCurve_Single animationCurve, UnityVersion version, IKeyframe_Single keyframe)
	{
		animationCurve.SetDefaultRotationOrderAndCurveLoopType();
		animationCurve.Curve.Capacity = 1;
		animationCurve.Curve.AddNew().CopyValues(keyframe);
	}

	public static void SetValues(this IAnimationCurve_Single animationCurve, UnityVersion version, IKeyframe_Single keyframe1, IKeyframe_Single keyframe2)
	{
		animationCurve.SetDefaultRotationOrderAndCurveLoopType();
		animationCurve.Curve.Capacity = 2;
		animationCurve.Curve.AddNew().CopyValues(keyframe1);
		animationCurve.Curve.AddNew().CopyValues(keyframe2);
	}

	public static void SetValues(this IAnimationCurve_Single animationCurve, UnityVersion version, IReadOnlyList<IKeyframe_Single> keyframes)
	{
		animationCurve.SetDefaultRotationOrderAndCurveLoopType();
		animationCurve.Curve.Capacity = keyframes.Count;
		for (int i = 0; i < keyframes.Count; i++)
		{
			animationCurve.Curve.AddNew().CopyValues(keyframes[i]);
		}
	}

	public static void SetValues(this IAnimationCurve_Single animationCurve, UnityVersion version, IReadOnlyList<IKeyframe_Single> keyframes, CurveLoopTypes preInfinity, CurveLoopTypes postInfinity)
	{
		animationCurve.PreInfinityE = preInfinity;
		animationCurve.PostInfinityE = postInfinity;
		animationCurve.RotationOrderE = RotationOrder.OrderZXY;
		animationCurve.Curve.Capacity = keyframes.Count;
		for (int i = 0; i < keyframes.Count; i++)
		{
			animationCurve.Curve.AddNew().CopyValues(keyframes[i]);
		}
	}

	public static void SetDefaultRotationOrderAndCurveLoopType(this IAnimationCurve_Vector3f animationCurve)
	{
		animationCurve.PreInfinityE = CurveLoopTypes.CycleWithOffset;
		animationCurve.PostInfinityE = CurveLoopTypes.CycleWithOffset;
		animationCurve.RotationOrderE = RotationOrder.OrderZXY;
	}

	public static void SetDefaultRotationOrderAndCurveLoopType(this IAnimationCurve_Quaternionf animationCurve)
	{
		animationCurve.PreInfinityE = CurveLoopTypes.CycleWithOffset;
		animationCurve.PostInfinityE = CurveLoopTypes.CycleWithOffset;
		animationCurve.RotationOrderE = RotationOrder.OrderZXY;
	}
}
