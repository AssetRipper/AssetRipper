using AssetRipper.IO.Endian;
using System.Numerics;

namespace AssetRipper.Processing.AnimationClips.Editor;

public sealed record class StreamedCurveKey
{
	public StreamedCurveKey() { }
	public StreamedCurveKey(int index, Vector3 coefficient, float value)
	{
		Index = index;
		Coefficient = coefficient;
		RightSidedLimit = value;
		LeftSidedLimit = RightSidedLimit;
	}

	public void Read(ref EndianSpanReader reader)
	{
		Index = reader.ReadInt32();
		Coefficient = new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		RightSidedLimit = reader.ReadSingle();
		LeftSidedLimit = RightSidedLimit;
	}

	public void CalculateSlopes(float time, float nextTime, StreamedCurveKey nextKey, bool UseNegInfSlopes)
	{
		if (Coefficient != default) // != (0,0,0) => regular slopes
		{
			double deltaTime = (double)nextTime - time;
			SetRegularSlopes(deltaTime, nextKey, out double _CoeffX, out double _CoeffY);

			if (UseNegInfSlopes && nextKey.Coefficient == default) // calculate next LSL only if next slope calculation needs it
			{
				nextKey.LeftSidedLimit = (_CoeffX + _CoeffY + Coefficient.Z) * deltaTime + RightSidedLimit;
				nextKey.ExactLeftSidedLimit = false;
			}
		}

		// set Special slope to this.OutSlope, because has priority over nextKey.InSlope on shaping the curve
		// nextKey.InSlope can stay Zero

		else if (UseNegInfSlopes)
		{
			// * there is one curve configuration that can fail and set incorrect slopes
			CalculateSpecialSlopes(nextKey); // Infinity, -Infinity or Zero
		}
		else
		{
			// should be a perfect reconstruction
			SetNonNegSpecialSlopes(nextKey); // Infinity or Zero
		}
	}

	private void SetRegularSlopes(double deltaX, StreamedCurveKey nextKey, out double _CoeffX, out double _CoeffY)
	{
		OutSlope = Coefficient.Z;
		_CoeffX = Coefficient.X * deltaX * deltaX;
		_CoeffY = Coefficient.Y * deltaX;
		nextKey.InSlope = (float)(3 * _CoeffX + 2 * _CoeffY + Coefficient.Z); // may need some kind of rounding if wanting to get an inSlope of exactly 0
	}

	private void SetNonNegSpecialSlopes(StreamedCurveKey nextKey)
	{
		if (RightSidedLimit != nextKey.RightSidedLimit)
		{
			OutSlope = float.PositiveInfinity;
			// nextKey.InSlope is already 0
		}
		// else { this.OutSlope and nextKey.InSlope are already 0 }
	}

	private void CalculateSpecialSlopes(StreamedCurveKey nextKey)
	{
		bool isDiscontinuous = HasDifferentLimits();
		if (RightSidedLimit == nextKey.RightSidedLimit)
		{
			// OutSlope IS Zero or -Infinity
			// but -Inf can also recreate Zero curves, so that will be used
			SetNegInfSlope(nextKey, isDiscontinuous);
			return;
		}

		if (isDiscontinuous) // * this estimation can be source of error
		{
			// OutSlope SHOULD be -Infinity
			SetNegInfSlope(nextKey, true);
			return;
		}

		// OutSlope SHOULD be Zero or +Infinity
		// but +Inf can also recreate Zero curves, so that will be used
		SetPosInfSlope();
	}

	private void SetNegInfSlope(StreamedCurveKey nextKey, bool isDiscontinuous)
	{
		OutSlope = float.NegativeInfinity;
		// nextKey.InSlope is already 0

		// set next LSL only if next slope calculation needs it
		if (nextKey.Coefficient == default)
		{
			nextKey.LeftSidedLimit = RightSidedLimit;
		}

		// fix Value for true -Infinity slopes
		if (isDiscontinuous)
		{
			RightSidedLimit = (float)LeftSidedLimit;
		}
	}

	private void SetPosInfSlope()
	{
		OutSlope = float.PositiveInfinity;
		// nextKey.InSlope is already 0

		/// Don't modify nextKey.LeftSidedLimit here!
		/// If nextKey.LeftSidedLimit is kept equal to nextKey.RightSidedLimit,
		/// then nextKey can't produce OutSlope of -Infinity
		/// +Inf outslope followed by -Inf outslope is an "illegal configuration".
	}

	private bool HasDifferentLimits()
	{
		if (RightSidedLimit != LeftSidedLimit)
		{
			if (ExactLeftSidedLimit)
			{
				return true;
			}

			// check percentage difference. this estimation can be source of error
			double diff = double.Abs((double)RightSidedLimit - LeftSidedLimit);
			double div = double.Max(double.Abs(LeftSidedLimit), float.Abs(RightSidedLimit));
			const float ROUNDING_ERROR = 5e-7f; // arbitrary small value
			/// normally the difference between Right and Left Sided Limits
			/// should be "big"/much greater than a rounding error.
			if (diff / div > ROUNDING_ERROR)
			{
				return true;
			}
		}
		return false;
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
	private double LeftSidedLimit { get; set; }
	/// <summary>
	/// Used only to calculate slopes.
	/// </summary>
	/// <remarks>
	/// Should be <c>false</c> if <c>LeftSidedLimit</c> was set from an arithmetic calculation
	/// instead of from <c>Read()</c>, meaning its not 100% reliable for equality comparison.
	/// </remarks>
	private bool ExactLeftSidedLimit { get; set; } = true;
	public float InSlope { get; private set; }
	public float OutSlope { get; private set; }
}
