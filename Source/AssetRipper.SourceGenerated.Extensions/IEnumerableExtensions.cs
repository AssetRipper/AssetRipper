namespace AssetRipper.SourceGenerated.Extensions;

public static class IEnumerableExtensions
{
	public static int IndexOf<T>(this IEnumerable<T> _this, Func<T, bool> predicate)
	{
		int index = 0;
		foreach (T t in _this)
		{
			if (predicate(t))
			{
				return index;
			}
			index++;
		}
		return -1;
	}
}
