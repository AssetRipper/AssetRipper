namespace AssetRipper.SourceGenerated.Extensions;

public static class IReadOnlyListExtensions
{
	public static int IndexOf<T>(this IReadOnlyList<T> _this, T value)
	{
		for (int i = 0; i < _this.Count; i++)
		{
			T t = _this[i];
			if (EqualityComparer<T>.Default.Equals(t, value))
			{
				return i;
			}
		}
		return -1;
	}

	public static bool Contains<T>(this IReadOnlyList<T> _this, T value)
	{
		return _this.IndexOf(value) > -1;
	}
}
