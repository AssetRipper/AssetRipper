namespace AssetRipper.AssemblyDumper.Extensions;

internal static class DictionaryExtensions
{
	public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
		where TKey : notnull
		where TValue : new()
	{
		if (!dict.TryGetValue(key, out TValue? value))
		{
			value = new TValue();
			dict.Add(key, value);
		}
		return value;
	}
}
