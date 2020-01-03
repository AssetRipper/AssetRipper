using System.Collections.Generic;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct StreamedCurveKey : IAssetReadable
	{
		public StreamedCurveKey(int index, float value, Vector3f coefs)
		{
			Index = index;
			Value = value;
			Coefficient = coefs;
		}

		public static StreamedCurveKey CalculateStreamedFrame(AnimationCurveTpl<Float> curve, int lhsIndex, int rhsIndex, float timeOffset)
		{
			IReadOnlyList<KeyframeTpl<Float>> keyframes = curve.Curve;
			KeyframeTpl<Float> lhs = keyframes[lhsIndex];
			int curveKeyIndex = lhsIndex;
			KeyframeTpl<Float> rhs = keyframes[rhsIndex];
			float frameTime = lhs.Time + timeOffset;
			//TimeEnd = rhs.Time + timeOffset;
			float deltaTime = rhs.Time - lhs.Time;
			if (deltaTime < 0.00009999999747378752)
			{
				deltaTime = 0.000099999997f;
			}
			float deltaValue = rhs.Value.Value - lhs.Value.Value;
			float inverseTime = 1.0f / (deltaTime * deltaTime);
			float outTangent = lhs.OutSlope.Value * deltaTime;
			float inTangent = rhs.InSlope.Value * deltaTime;
			float curveKeyCoefX = (inTangent + outTangent - deltaValue - deltaValue) * inverseTime / deltaTime;
			float curveKeyCoefY = inverseTime * (deltaValue + deltaValue + deltaValue - outTangent - outTangent - inTangent);
			float curveKeyCoefZ = lhs.OutSlope.Value;
			float curveKeyValue = lhs.Value.Value;
			if (lhs.OutSlope.Value == float.PositiveInfinity || rhs.InSlope.Value == float.PositiveInfinity)
			{
				curveKeyCoefX = 0.0f;
				curveKeyCoefY = 0.0f;
				curveKeyCoefZ = 0.0f;
				curveKeyValue = lhs.Value.Value;
			}
			Vector3f curveKeyCoef = new Vector3f(curveKeyCoefX, curveKeyCoefY, curveKeyCoefZ);
			StreamedCurveKey curveKey = new StreamedCurveKey(curveKeyIndex, curveKeyValue, curveKeyCoef);
			return curveKey;
		}

		/// <summary>
		/// Calculate value between two KeyframeTpl<Float> at given time
		/// </summary>
		/// <param name="deltaTimeFraction">(time - leftTime) / (rightTime - leftTime)</param>
		/// <param name="leftVaue">lhs.Value</param>
		/// <param name="outTangent">lhs.OutSlope * (rightTime - leftTime)</param>
		/// <param name="rightValue">rhs.Value</param>
		/// <param name="inTangent">rhs.OutSlope * (rightTime - leftTime)</param>
		/// <returns>Value between two keyframes</returns>
		public static float HermiteInterpolate(float deltaTimeFraction, float leftVaue, float outTangent, float rightValue, float inTangent)
		{
			float tt = deltaTimeFraction * deltaTimeFraction;
			float ttt = tt * deltaTimeFraction;
			float tttx2 = ttt * 2.0f;
			float ttx3 = tt * 3.0f;
			float v1 = deltaTimeFraction + ttt - 2.0f * tt * outTangent + (tttx2 - ttx3 + 1.0f) * leftVaue;
			float v2 = (ttt - tt) * inTangent;
			float v3 = ttx3 - tttx2;
			return v1 + v2 + v3 * rightValue;
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
			float inTangent = deltaValue + deltaValue + deltaValue - outTangent - outTangent - Coefficient.Y / inverseTime;
			return inTangent / deltaTime;
		}

		public void Read(AssetReader reader)
		{
			Index = reader.ReadInt32();
			Coefficient.Read(reader);
			Value = reader.ReadSingle();
		}

		public float OutSlope => Coefficient.Z;

		public int Index { get; set; }
		public float Value { get; set; }

		public Vector3f Coefficient;
	}
}
