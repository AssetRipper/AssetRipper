using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.SourceGenerated.Subclasses.AnimationCurve_float;
using AssetRipper.SourceGenerated.Subclasses.Keyframe_float;
using System.Collections.Generic;

namespace AssetRipper.Library.Exporters.AnimatorControllers.Editor
{
	public sealed class StreamedCurveKey : IAssetReadable
	{
		public StreamedCurveKey() { }
		public StreamedCurveKey(int index, float value, IVector3f coefs)
		{
			Index = index;
			Value = value;
			Coefficient = coefs;
		}

		public static StreamedCurveKey CalculateStreamedFrame(IAnimationCurve_float curve, int lhsIndex, int rhsIndex, float timeOffset)
		{
			IReadOnlyList<IKeyframe_float> keyframes = curve.Curve;
			IKeyframe_float lhs = keyframes[lhsIndex];
			int curveKeyIndex = lhsIndex;
			IKeyframe_float rhs = keyframes[rhsIndex];
			float frameTime = lhs.Time + timeOffset;
			//TimeEnd = rhs.Time + timeOffset;
			float deltaTime = rhs.Time - lhs.Time;
			if (deltaTime < 0.00009999999747378752)
			{
				deltaTime = 0.000099999997f;
			}
			float deltaValue = rhs.Value - lhs.Value;
			float inverseTime = 1.0f / (deltaTime * deltaTime);
			float outTangent = lhs.OutSlope * deltaTime;
			float inTangent = rhs.InSlope * deltaTime;
			float curveKeyCoefX = (inTangent + outTangent - deltaValue - deltaValue) * inverseTime / deltaTime;
			float curveKeyCoefY = inverseTime * (deltaValue + deltaValue + deltaValue - outTangent - outTangent - inTangent);
			float curveKeyCoefZ = lhs.OutSlope;
			float curveKeyValue = lhs.Value;
			if (lhs.OutSlope == float.PositiveInfinity || rhs.InSlope == float.PositiveInfinity)
			{
				curveKeyCoefX = 0.0f;
				curveKeyCoefY = 0.0f;
				curveKeyCoefZ = 0.0f;
				curveKeyValue = lhs.Value;
			}
			Vector3f curveKeyCoef = new Vector3f(curveKeyCoefX, curveKeyCoefY, curveKeyCoefZ);
			StreamedCurveKey curveKey = new StreamedCurveKey(curveKeyIndex, curveKeyValue, curveKeyCoef);
			return curveKey;
		}

		public void Read(AssetReader reader)
		{
			Index = reader.ReadInt32();
			Coefficient.Read(reader);
			Value = reader.ReadSingle();
		}

		/// <summary>
		/// Calculate value between two KeyframeTpl Float at given time
		/// </summary>
		/// <remarks>
		/// This calculates a unit interval cubic Hermite spline.<br />
		/// <see href="https://en.wikipedia.org/wiki/Cubic_Hermite_spline"/><br />
		/// <see href="https://en.wikipedia.org/wiki/Hermite_interpolation"/>
		/// </remarks>
		/// <param name="deltaTimeFraction">(time - leftTime) / (rightTime - leftTime)</param>
		/// <param name="leftValue">lhs.Value</param>
		/// <param name="leftTangent">lhs.OutSlope * (rightTime - leftTime)</param>
		/// <param name="rightValue">rhs.Value</param>
		/// <param name="rightTangent">rhs.OutSlope * (rightTime - leftTime)</param>
		/// <returns>Value between two keyframes</returns>
		public static float HermiteInterpolate(float deltaTimeFraction, float leftValue, float leftTangent, float rightValue, float rightTangent)
		{
			float tt = deltaTimeFraction * deltaTimeFraction;
			float ttt = tt * deltaTimeFraction;
			float tttx2 = ttt * 2.0f;
			float ttx3 = tt * 3.0f;
			float v1 = ((deltaTimeFraction + ttt - (2.0f * tt)) * leftTangent) + ((tttx2 - ttx3 + 1.0f) * leftValue);
			float v2 = (ttt - tt) * rightTangent;
			float v3 = ttx3 - tttx2;
			return v1 + v2 + (v3 * rightValue);
		}

		public float CalculateNextInSlope(float deltaTime, float nextValue)
		{
			if (deltaTime >= 3.40282347e38f)
			{
				return 0;
			}
			if (deltaTime < 0.00009999999747378752)
			{
				deltaTime = 0.000099999997f;
			}
			float deltaValue = nextValue - Value;
			float inverseTime = 1.0f / (deltaTime * deltaTime);
			float outTangent = OutSlope * deltaTime;
			float inTangent = deltaValue + deltaValue + deltaValue - outTangent - outTangent - (Coefficient.Y / inverseTime);
			return inTangent / deltaTime;
		}

		public float OutSlope => Coefficient.Z;

		public int Index { get; set; }
		public float Value { get; set; }
		public IVector3f Coefficient { get; set; } = new Vector3f();
	}
}
