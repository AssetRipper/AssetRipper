namespace AssetRipper.SourceGenerated.Extensions;

public static class IDictionaryExtensions
{
	public static void AddRange<T1, T2>(this IDictionary<T1, T2> _this, IReadOnlyDictionary<T1, T2> source)
	{
		foreach (KeyValuePair<T1, T2> argument in source)
		{
			_this.Add(argument.Key, argument.Value);
		}
	}

	public static T2 GetOrAdd<T1, T2>(this IDictionary<T1, T2> _this, T1 key) where T1 : notnull where T2 : new()
	{
		if (!_this.TryGetValue(key, out T2? value))
		{
			value = new();
			_this.Add(key, value);
		}
		return value;
	}

	public static T2 GetOrAdd<T1, T2>(this IDictionary<T1, T2> _this, T1 key, Func<T2> factory) where T1 : notnull
	{
		if (!_this.TryGetValue(key, out T2? value))
		{
			value = factory();
			_this.Add(key, value);
		}
		return value;
	}

	public static TValue? TryGetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> _this, TKey key) where TKey : notnull
	{
		_this.TryGetValue(key, out TValue? value);
		return value;
	}

	/// <remarks>
	/// .NET Core 3.0+ only. <see href="https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.getenumerator?view=net-7.0#remarks"/>
	/// </remarks>
	public static void RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> source, Predicate<KeyValuePair<TKey, TValue>> predicate) where TKey : notnull
	{
		foreach (KeyValuePair<TKey, TValue> pair in source)
		{
			if (predicate(pair))
			{
				source.Remove(pair.Key);
			}
		}
	}
}
