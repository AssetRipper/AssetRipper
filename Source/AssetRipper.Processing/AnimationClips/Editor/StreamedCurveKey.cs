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
			RightSidedLimit = LeftSidedLimit = value;
		}

		public void Read(ref EndianSpanReader reader)
		{
			Index = reader.ReadInt32();
			Coefficient = new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			RightSidedLimit = reader.ReadSingle();
			LeftSidedLimit = RightSidedLimit;
		}

		public void CalculateSlopes(float time, float nextTime, StreamedCurveKey nextKey)
		{
			if (Coefficient != default) // != (0,0,0) => regular slopes
			{
				OutSlope = Coefficient.Z;
				float deltaX = nextTime - time;
				float _CoeffX = Coefficient.X * deltaX * deltaX;
				float _CoeffY = Coefficient.Y * deltaX;
				nextKey.InSlope = 3 * _CoeffX + 2 * _CoeffY + Coefficient.Z; // may need some kind of rounding if wanting to get an inSlope of exactly 0
				//nextKey.inSlope = float.Round(nextKey.inSlope, 4); // amount of decimal places may be more dynamic
				if (nextKey.Coefficient == default) // calculate LSL only if next slope calculation needs it
				{
					nextKey.LeftSidedLimit = (_CoeffX + _CoeffY + Coefficient.Z) * deltaX + RightSidedLimit;
				}
				return;
			}
			if (RightSidedLimit != LeftSidedLimit)
			{
				// check percentage difference
				float diff = RightSidedLimit > LeftSidedLimit ? RightSidedLimit - LeftSidedLimit : LeftSidedLimit - RightSidedLimit;
				float AbsVal = RightSidedLimit < 0 ? -RightSidedLimit : RightSidedLimit;
				float AbsLSV = LeftSidedLimit < 0 ? -LeftSidedLimit : LeftSidedLimit;
				float div = AbsVal > AbsLSV ? AbsVal : AbsLSV;
				const float ROUNDING_ERROR = 1e-5f; // arbitrary small value
				// normally the difference between Right and Left Sided Limits should be "big"/much greater than a rounding error.
				// if the check fails and INCORRECTLY skips this IF block, the next statements
				// will still produce a good approximation for the expected curve
				if (diff/div > ROUNDING_ERROR)
				{
					OutSlope = float.NegativeInfinity;
					nextKey.InSlope = 0f;
					nextKey.LeftSidedLimit = RightSidedLimit;
					RightSidedLimit = LeftSidedLimit;
					return;
				}
			}
			if (RightSidedLimit == nextKey.RightSidedLimit)
			{
				OutSlope = 0f;
				nextKey.InSlope = 0f;
				nextKey.LeftSidedLimit = RightSidedLimit;
				return;
			}
			OutSlope = float.PositiveInfinity;
			nextKey.InSlope = 0f;
			// don't do nextKey.LeftSidedLimit=RightSidedLimit here, because having 2 consecutive keys
			// with outSlope +Inf and -Inf is illegal (Editor corrects it)
		}

		/// <summary>
		/// Index for its GenericBinding inside AnimationClip's BindingConstant.
		/// </summary>
		public int Index { get; private set; }
		/// <summary>
		/// Coefficients of the Cubic Bezier Equation between this Key and the next, for the same Curve.
		/// </summary>
		public Vector3 Coefficient { get; private set; }
		/// <summary>
		/// Value of the binded property during this keyframe.
		/// </summary>
		/// /// <remarks>
		/// This value could change after Slope calculation.
		/// </remarks>
		public float Value { get => RightSidedLimit; private set => RightSidedLimit = value; }
		/// <summary>
		/// Value of the key, when approached from its right side.
		/// </summary>
		/// <remarks>
		/// outSlope=-Infinity creates a discontinuity on the curve,
		/// making the CurveKey value differ from its left side.
		/// </remarks>
		private float RightSidedLimit { get; set; }
		/// <summary>
		/// Value of the key, when approached from its left side
		/// </summary>
		/// <remarks>
		/// outSlope=-Infinity creates a discontinuity on the curve,
		/// making the CurveKey value differ from its right side.
		/// </remarks>
		private float LeftSidedLimit { get; set; }
		public float InSlope { get; private set; }
		public float OutSlope { get; private set; }
	}
}
