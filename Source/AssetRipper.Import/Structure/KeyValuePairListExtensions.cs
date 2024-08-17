namespace AssetRipper.Import.Structure;

internal static class KeyValuePairListExtensions
{
	public static IEnumerable<TValue> Values<TKey, TValue>(this List<KeyValuePair<TKey, TValue>> list)
	{
		foreach (KeyValuePair<TKey, TValue> pair in list)
		{
			yield return pair.Value;
		}
	}

	public static void Add<TKey, TValue>(this List<KeyValuePair<TKey, TValue>> list, TKey key, TValue value)
	{
		list.Add(new KeyValuePair<TKey, TValue>(key, value));
	}
}
