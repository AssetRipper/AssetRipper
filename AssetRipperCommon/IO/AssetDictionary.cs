using System.Collections.Generic;

namespace AssetRipper.Core.IO
{
	/// <summary>
	/// A dictionary class supporting non-unique keys
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
	public class AssetDictionary<TKey, TValue> : List<NullableKeyValuePair<TKey, TValue>>
	{
		public void Add(TKey key, TValue value) => Add(new NullableKeyValuePair<TKey, TValue>(key, value));
	}
}
