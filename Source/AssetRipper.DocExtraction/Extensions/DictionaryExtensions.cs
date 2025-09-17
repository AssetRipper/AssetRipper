namespace AssetRipper.DocExtraction.Extensions;

internal static class DictionaryExtensions
{
	public static TValue? TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
	{
		dictionary.TryGetValue(key, out TValue? value);
		return value;
	}

	public static bool EqualByContent<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> other) where TKey : notnull
	{
		if (dictionary.GetType() != other.GetType() || dictionary.Count != other.Count)
		{
			return false;
		}

		EqualityComparer<TValue> comparer = EqualityComparer<TValue>.Default;
		foreach ((TKey key, TValue value) in dictionary)
		{
			if (!other.TryGetValue(key, out TValue? otherValue) || !comparer.Equals(value, otherValue))
			{
				return false;
			}
		}

		return true;
	}

	public static int GetHashCodeByContent<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull
	{
		int hashCode = 0;
		foreach ((TKey key, _) in dictionary)
		{
			hashCode ^= key.GetHashCode();
		}
		return hashCode;
	}
}