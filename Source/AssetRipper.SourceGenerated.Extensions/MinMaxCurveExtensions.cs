using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.MinMaxCurve;

namespace AssetRipper.SourceGenerated.Extensions;

public static class MinMaxCurveExtensions
{
	public static void SetValues(this IMinMaxCurve curve, UnityVersion version, float value)
	{
		curve.SetValues(version, ParticleSystemCurveMode.Constant, value, value, 1.0f, 1.0f);
	}

	public static void SetValues(this IMinMaxCurve curve, UnityVersion version, float minValue, float maxValue)
	{
		curve.SetValues(version, ParticleSystemCurveMode.Constant, minValue, maxValue, 1.0f, 1.0f);
	}

	public static void SetValues(this IMinMaxCurve curve, UnityVersion version, float minValue, float maxValue, float minCurve, float maxCurve)
	{
		curve.SetValues(version, ParticleSystemCurveMode.Constant, minValue, maxValue, minCurve, maxCurve);
	}

	public static void SetValues(this IMinMaxCurve curve, UnityVersion version, float minValue, float maxValue, float minCurve, float maxCurve1, float maxCurve2)
	{
		curve.SetMinMaxState(ParticleSystemCurveMode.Curve);
		curve.Scalar = maxValue;
		curve.MinScalar = minValue;

		curve.MinCurve.SetValues(version, minCurve, KeyframeExtensions.DefaultFloatWeight);
		curve.MaxCurve.SetValues(version, maxCurve1, 0.0f, 1.0f, maxCurve2, 1.0f, 0.0f, KeyframeExtensions.DefaultFloatWeight);
	}

	public static void SetValues(this IMinMaxCurve curve, UnityVersion version, ParticleSystemCurveMode mode, float minValue, float maxValue, float minCurve, float maxCurve)
	{
		curve.SetMinMaxState(mode);
		curve.MinScalar = minValue;
		curve.Scalar = maxValue;

		curve.MinCurve.SetValues(version, minCurve, KeyframeExtensions.DefaultFloatWeight);
		curve.MaxCurve.SetValues(version, maxCurve, KeyframeExtensions.DefaultFloatWeight);
	}

	public static ParticleSystemCurveMode GetMinMaxState(this IMinMaxCurve curve)
	{
		return curve.Has_MinMaxState_Int16()
			? unchecked((ParticleSystemCurveMode)curve.MinMaxState_Int16)
			: (ParticleSystemCurveMode)curve.MinMaxState_UInt16;
	}

	public static void SetMinMaxState(this IMinMaxCurve curve, ParticleSystemCurveMode mode)
	{
		curve.MinMaxState_Int16 = unchecked((short)mode);
		curve.MinMaxState_UInt16 = (ushort)mode;
	}

	private static float GetExportScalar(this IMinMaxCurve curve)
	{
		if (curve.Has_MinScalar())
		{
			return curve.Scalar;
		}
		else
		{
			if (curve.GetMinMaxState() == ParticleSystemCurveMode.TwoConstants)
			{
				return curve.Scalar * curve.MaxCurve.Curve[0].Value;
			}
			else
			{
				return curve.Scalar;
			}
		}
	}

	private static float GetExportMinScalar(this IMinMaxCurve curve)
	{
		if (curve.Has_MinScalar())
		{
			return curve.MinScalar;
		}
		else
		{
			if (curve.GetMinMaxState() == ParticleSystemCurveMode.TwoConstants)
			{
				return curve.Scalar * curve.MinCurve.Curve[0].Value;
			}
			else
			{
				return curve.Scalar;
			}
		}
	}
}
