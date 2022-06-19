namespace AssetRipper.Core.Equality
{
	public static class NearEquality
	{
		public static bool AlmostEqualByProportion(float value1, float value2, float maximumProportion)
		{
			if (float.IsNaN(value1) || float.IsNaN(value2))
			{
				return float.IsNaN(value1) && float.IsNaN(value2);
			}

			if (float.IsPositiveInfinity(value1) || float.IsPositiveInfinity(value2))
			{
				return float.IsPositiveInfinity(value1) && float.IsPositiveInfinity(value2);
			}

			if (float.IsNegativeInfinity(value1) || float.IsNegativeInfinity(value2))
			{
				return float.IsNegativeInfinity(value1) && float.IsNegativeInfinity(value2);
			}

			if (value1 == 0f || value2 == 0f)
			{
				return value1 == value2;
			}

			float proportion = (value1 > value2 ? value1 / value2 : value2 / value1) - 1f;

			return 0 <= proportion && proportion <= maximumProportion;
		}

		public static bool AlmostEqualByDeviation(float value1, float value2, float maximumDeviation)
		{
			if (float.IsNaN(value1) || float.IsNaN(value2))
			{
				return float.IsNaN(value1) && float.IsNaN(value2);
			}

			if (float.IsPositiveInfinity(value1) || float.IsPositiveInfinity(value2))
			{
				return float.IsPositiveInfinity(value1) && float.IsPositiveInfinity(value2);
			}

			if (float.IsNegativeInfinity(value1) || float.IsNegativeInfinity(value2))
			{
				return float.IsNegativeInfinity(value1) && float.IsNegativeInfinity(value2);
			}

			float deviation = System.Math.Abs(value1 - value2);

			return deviation <= maximumDeviation;
		}

		public static bool AlmostEqualByProportion(double value1, double value2, float maximumProportion)
		{
			if (double.IsNaN(value1) || double.IsNaN(value2))
			{
				return double.IsNaN(value1) && double.IsNaN(value2);
			}

			if (double.IsPositiveInfinity(value1) || double.IsPositiveInfinity(value2))
			{
				return double.IsPositiveInfinity(value1) && double.IsPositiveInfinity(value2);
			}

			if (double.IsNegativeInfinity(value1) || double.IsNegativeInfinity(value2))
			{
				return double.IsNegativeInfinity(value1) && double.IsNegativeInfinity(value2);
			}

			if (value1 == 0d || value2 == 0d)
			{
				return value1 == value2;
			}

			double proportion = (value1 > value2 ? value1 / value2 : value2 / value1) - 1d;

			return 0 <= proportion && proportion <= maximumProportion;
		}

		public static bool AlmostEqualByDeviation(double value1, double value2, float maximumDeviation)
		{
			if (double.IsNaN(value1) || double.IsNaN(value2))
			{
				return double.IsNaN(value1) && double.IsNaN(value2);
			}

			if (double.IsPositiveInfinity(value1) || double.IsPositiveInfinity(value2))
			{
				return double.IsPositiveInfinity(value1) && double.IsPositiveInfinity(value2);
			}

			if (double.IsNegativeInfinity(value1) || double.IsNegativeInfinity(value2))
			{
				return double.IsNegativeInfinity(value1) && double.IsNegativeInfinity(value2);
			}

			double deviation = System.Math.Abs(value1 - value2);

			return deviation <= maximumDeviation;
		}
	}
}
