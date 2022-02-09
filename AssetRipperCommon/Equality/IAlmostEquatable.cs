namespace AssetRipper.Core.Equality
{
	public interface IAlmostEquatable
	{
		/// <summary>
		/// Check if two objects are almost equal to each other by proportion
		/// </summary>
		/// <param name="value">The value to compare with</param>
		/// <param name="maximumProportion"></param>
		/// <remarks>
		/// Float comparisons are done by <see cref="NearEquality.AlmostEqualByProportion(float, float, float)"/><br/>
		/// Double comparisons are done by <see cref="NearEquality.AlmostEqualByProportion(double, double, float)"/>
		/// </remarks>
		/// <returns>True if the objects are equal or almost equal by proportion</returns>
		bool AlmostEqualByProportion(object value, float maximumProportion);

		/// <summary>
		/// Check if two objects are almost equal to each other by deviation
		/// </summary>
		/// <param name="value">The value to compare with</param>
		/// <param name="maximumDeviation">The positive maximum value deviation between two near equal decimal values</param>
		/// <remarks>
		/// Float comparisons are done by <see cref="NearEquality.AlmostEqualByDeviation(float, float, float)"/><br/>
		/// Double comparisons are done by <see cref="NearEquality.AlmostEqualByDeviation(double, double, float)"/>
		/// </remarks>
		/// <returns>True if the objects are equal or almost equal by deviation</returns>
		bool AlmostEqualByDeviation(object value, float maximumDeviation);
	}
}
