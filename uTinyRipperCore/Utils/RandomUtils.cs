using System;

namespace uTinyRipper
{
	public static class RandomUtils
	{
		/// <summary>
		/// Returns a random integer that is within a specified range.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer greater than or equal to minValue and less than maxValue;
		/// that is, the range of return values includes minValue but not maxValue. If minValue 
		/// equals maxValue, minValue is returned.
		/// </returns>
		public static int Next()
		{
			return s_random.Next();
		}
		/// <summary>
		/// Returns a random integer that is within a specified range.
		/// </summary>
		/// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater
		/// than or equal to minValue.</param>
		/// <returns>
		/// A 32-bit signed integer greater than or equal to minValue and less than maxValue;
		/// that is, the range of return values includes minValue but not maxValue. If minValue 
		/// equals maxValue, minValue is returned.
		/// </returns>
		public static int Next(int maxValue)
		{
			return s_random.Next(maxValue);
		}

		/// <summary>
		/// Returns a random integer that is within a specified range.
		/// </summary>
		/// <param name="minValue">The inclusive lower bound of the random number returned.</param>
		/// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater
		/// than or equal to minValue.</param>
		/// <returns>
		/// A 32-bit signed integer greater than or equal to minValue and less than maxValue;
		/// that is, the range of return values includes minValue but not maxValue. If minValue 
		/// equals maxValue, minValue is returned.
		/// </returns>
		public static int Next(int minValue, int maxValue)
		{
			return s_random.Next(minValue, maxValue);
		}

		private static readonly Random s_random = new Random();
	}
}
