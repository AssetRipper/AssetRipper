using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.SourceGenerated.Subclasses.AnimationCurve_float;
using AssetRipper.SourceGenerated.Subclasses.AnimationCurve_Quaternionf;
using AssetRipper.SourceGenerated.Subclasses.AnimationCurve_Vector3f;
using AssetRipper.SourceGenerated.Subclasses.Keyframe_float;
using System.Collections.Generic;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AnimationCurveExtensions
	{
		public static void SetDefaultRotationOrderAndCurveLoopType(this IAnimationCurve_float animationCurve)
		{
			animationCurve.PreInfinity = (int)CurveLoopTypes.CycleWithOffset;
			animationCurve.PostInfinity = (int)CurveLoopTypes.CycleWithOffset;
			animationCurve.RotationOrder = (int)RotationOrder.OrderZXY;
		}

		public static void SetValues(this IAnimationCurve_float animationCurve, UnityVersion version, float defaultValue, float defaultWeight)
		{
			animationCurve.SetDefaultRotationOrderAndCurveLoopType();
			animationCurve.Curve.Capacity = 2;
			animationCurve.Curve.AddNew().SetValues(version, 0.0f, defaultValue, defaultWeight);
			animationCurve.Curve.AddNew().SetValues(version, 1.0f, defaultValue, defaultWeight);
		}

		public static void SetValues(this IAnimationCurve_float animationCurve, UnityVersion version, float value1, float value2, float defaultWeight)
		{
			animationCurve.SetDefaultRotationOrderAndCurveLoopType();
			animationCurve.Curve.Capacity = 2;
			animationCurve.Curve.AddNew().SetValues(version, 0.0f, value1, defaultWeight);
			animationCurve.Curve.AddNew().SetValues(version, 1.0f, value2, defaultWeight);
		}

		public static void SetValues(this IAnimationCurve_float animationCurve, UnityVersion version, float value1, float inSlope1, float outSlope1, float value2, float inSlope2, float outSlope2, float defaultWeight)
		{
			animationCurve.SetDefaultRotationOrderAndCurveLoopType();
			animationCurve.Curve.Capacity = 2;
			animationCurve.Curve.AddNew().SetValues(version, 0.0f, value1, inSlope1, outSlope1, defaultWeight);
			animationCurve.Curve.AddNew().SetValues(version, 1.0f, value2, inSlope2, outSlope2, defaultWeight);
		}

		public static void SetValues(this IAnimationCurve_float animationCurve, UnityVersion version, IKeyframe_float keyframe)
		{
			animationCurve.SetDefaultRotationOrderAndCurveLoopType();
			animationCurve.Curve.Capacity = 1;
			animationCurve.Curve.AddNew().CopyValues(keyframe);
		}

		public static void SetValues(this IAnimationCurve_float animationCurve, UnityVersion version, IKeyframe_float keyframe1, IKeyframe_float keyframe2)
		{
			animationCurve.SetDefaultRotationOrderAndCurveLoopType();
			animationCurve.Curve.Capacity = 2;
			animationCurve.Curve.AddNew().CopyValues(keyframe1);
			animationCurve.Curve.AddNew().CopyValues(keyframe2);
		}

		public static void SetValues(this IAnimationCurve_float animationCurve, UnityVersion version, IReadOnlyList<IKeyframe_float> keyframes)
		{
			animationCurve.SetDefaultRotationOrderAndCurveLoopType();
			animationCurve.Curve.Capacity = keyframes.Count;
			for (int i = 0; i < keyframes.Count; i++)
			{
				animationCurve.Curve.AddNew().CopyValues(keyframes[i]);
			}
		}

		public static void SetValues(this IAnimationCurve_float animationCurve, UnityVersion version, IReadOnlyList<IKeyframe_float> keyframes, CurveLoopTypes preInfinity, CurveLoopTypes postInfinity)
		{
			animationCurve.PreInfinity = (int)preInfinity;
			animationCurve.PostInfinity = (int)postInfinity;
			animationCurve.RotationOrder = (int)RotationOrder.OrderZXY;
			animationCurve.Curve.Capacity = keyframes.Count;
			for (int i = 0; i < keyframes.Count; i++)
			{
				animationCurve.Curve.AddNew().CopyValues(keyframes[i]);
			}
		}

		public static void SetDefaultRotationOrderAndCurveLoopType(this IAnimationCurve_Vector3f animationCurve)
		{
			animationCurve.PreInfinity = (int)CurveLoopTypes.CycleWithOffset;
			animationCurve.PostInfinity = (int)CurveLoopTypes.CycleWithOffset;
			animationCurve.RotationOrder = (int)RotationOrder.OrderZXY;
		}

		public static void SetDefaultRotationOrderAndCurveLoopType(this IAnimationCurve_Quaternionf animationCurve)
		{
			animationCurve.PreInfinity = (int)CurveLoopTypes.CycleWithOffset;
			animationCurve.PostInfinity = (int)CurveLoopTypes.CycleWithOffset;
			animationCurve.RotationOrder = (int)RotationOrder.OrderZXY;
		}
	}
}
