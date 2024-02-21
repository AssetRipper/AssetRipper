using AssetRipper.IO.Endian;
using System.Numerics;

namespace AssetRipper.Processing.AnimationClips.Editor
{
	public sealed record class StreamedCurveKey
	{
		public StreamedCurveKey() { }
		public StreamedCurveKey(int index, Vector3 coefficient, float value)
		{
			Index = index;
			Coefficient = coefficient;
			Value = LSV = value;
		}

		public void Read(ref EndianSpanReader reader)
		{
			Index = reader.ReadInt32();
			Coefficient = new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			Value = reader.ReadSingle();
			LSV = Value;
		}

		public void CalculateSlopes(float time, float nextTime, StreamedCurveKey nextKey)
		{
			if (Coefficient != default) // != (0,0,0) => regular slopes
			{
				outSlope = Coefficient.Z;
				float deltaX = nextTime - time;
				float _CoeffX = Coefficient.X * deltaX * deltaX;
				float _CoeffY = Coefficient.Y * deltaX;
				nextKey.inSlope = 3 * _CoeffX + 2 * _CoeffY + Coefficient.Z;
				//nextKey.inSlope = float.Round(nextKey.inSlope, 4); // amount of decimal places may be more dynamic
				if (nextKey.Coefficient == default) // calculate LSV only if next slope calculation needs it
				{
					nextKey.LSV = (_CoeffX + _CoeffY + Coefficient.Z) * deltaX + Value;
				}
				return;
			}
			if (Value != LSV)
			{
				// check percentage difference
				float diff = Value > LSV ? Value - LSV : LSV - Value;
				float AbsVal = Value < 0 ? -Value : Value;
				float AbsLSV = LSV < 0 ? -LSV : LSV;
				float div = AbsVal > AbsLSV ? AbsVal : AbsLSV;
				float ROUNDING_ERROR = 1e-5f;
				// normally the difference between Value and LSV should be "big"/much greater than a rounding error.
				// if the check fails and INCORRECTLY skips this IF block, the next statements
				// will still produce a good approximation for the expected curve
				if (diff/div > ROUNDING_ERROR)
				{
					outSlope = float.NegativeInfinity;
					nextKey.inSlope = 0f;
					nextKey.LSV = Value;
					Value = LSV;
					return;
				}
			}
			if (Value == nextKey.Value)
			{
				outSlope = 0f;
				nextKey.inSlope = 0f;
				nextKey.LSV = Value;
				return;
			}
			outSlope = float.PositiveInfinity;
			nextKey.inSlope = 0f;
			// don't do nextKey.LSV=Value here, because having 2 consecutive keys
			// with outSlope +Inf and -Inf is illegal (Editor corrects it)
		}

		/// <summary>
		/// Index for its GenericBinding inside AnimationClip's BindingConstant.
		/// </summary>
		public int Index { get; set; }
		/// <summary>
		/// Coefficients of the Cubic Bezier Equation between this Key and the next, for the same Curve.
		/// </summary>
		public Vector3 Coefficient { get; set; }
		/// <summary>
		/// Value of the key, when approached from its right side.
		/// </summary>
		/// <remarks>
		/// outSlope=-Infinity creates a discontinuity on the curve,
		/// making the CurveKey value differ from its left side.
		/// </remarks>
		public float Value { get; set; }
		/// <summary>
		/// Value of the key, when approached from its left side
		/// </summary>
		/// <remarks>
		/// outSlope=-Infinity creates a discontinuity on the curve,
		/// making the CurveKey value differ from its right side.
		/// </remarks>
		public float LSV { get; set; }
		public float inSlope = 0;
		public float outSlope = 0;
	}
}
