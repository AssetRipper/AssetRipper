using System.Collections.Generic;

namespace AssetRipper.Core.IO
{
	public class NullableKeyValuePair<TKey, TValue>
	{
		public TKey Key { get; set; }
		public TValue Value { get; set; }
		
		public NullableKeyValuePair() { }

		public NullableKeyValuePair(TKey key, TValue value)
		{
			Key = key;
			Value = value;
		}

		public NullableKeyValuePair(KeyValuePair<TKey,TValue> pair)
		{
			Key = pair.Key;
			Value = pair.Value;
		}

		public static implicit operator KeyValuePair<TKey, TValue>(NullableKeyValuePair<TKey, TValue> nullable)
		{
			return nullable == null ? default : new KeyValuePair<TKey, TValue>(nullable.Key, nullable.Value);
		}

		public static implicit operator NullableKeyValuePair<TKey, TValue>(KeyValuePair<TKey, TValue> nonnullable)
		{
			return new NullableKeyValuePair<TKey, TValue>(nonnullable);
		}
	}
}
