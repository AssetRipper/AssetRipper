namespace AssetRipper.Numerics;

internal static class IComparableExtensions
{
	public static bool IsLess<T>(this T left, T right) where T : IComparable<T>
	{
		return left.CompareTo(right) < 0;
	}

	public static bool IsLessEqual<T>(this T left, T right) where T : IComparable<T>
	{
		return left.CompareTo(right) <= 0;
	}

	public static bool IsGreater<T>(this T left, T right) where T : IComparable<T>
	{
		return left.CompareTo(right) > 0;
	}

	public static bool IsGreaterEqual<T>(this T left, T right) where T : IComparable<T>
	{
		return left.CompareTo(right) >= 0;
	}
}
