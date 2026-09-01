namespace AssetRipper.Import.Structure;

internal static class KeyValuePairHashSetExtensions
{
	public static IEnumerable<TValue> Values<TKey, TValue>(this HashSet<KeyValuePair<TKey, TValue>> set)
	{
		return set.Select(pair => pair.Value);
	}

	public static bool Add<TKey, TValue>(this HashSet<KeyValuePair<TKey, TValue>> set, TKey key, TValue value)
	{
		return set.Add(new KeyValuePair<TKey, TValue>(key, value));
	}
}
