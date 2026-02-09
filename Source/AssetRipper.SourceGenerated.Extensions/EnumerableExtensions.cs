namespace AssetRipper.SourceGenerated.Extensions;

public static class EnumerableExtensions
{
	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : notnull
	{
		foreach (T? item in enumerable)
		{
			if (item is not null)
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<T> ThrowIfNull<T>(this IEnumerable<T?> enumerable) where T : notnull
	{
		foreach (T? item in enumerable)
		{
			if (item is null)
			{
				throw new NullReferenceException();
			}
			yield return item;
		}
	}

	public static IEnumerable<T> MaybeAppend<T>(this IEnumerable<T> enumerable, T? item)
	{
		return item is not null ? enumerable.Append(item) : enumerable;
	}
}
