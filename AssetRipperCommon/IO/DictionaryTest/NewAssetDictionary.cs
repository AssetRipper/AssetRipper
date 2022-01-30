using System;
using System.Collections.Generic;

namespace AssetRipper.Core.IO.DictionaryTest
{
	public class NewAssetDictionary<TKey, TValue> : IAccessDictionary<TKey, TValue>
		where TKey : new() 
		where TValue : new()
	{
		private TKey[] keys;
		private TValue[] values;

		public int Count { get; private set; } = 0;

		public int Capacity => keys.Length;

		private KeyEnumerable keyEnumerable;
		public IEnumerable<TKey> Keys => keyEnumerable ??= new KeyEnumerable(this);

		private ValueEnumerable valueEnumerable;
		public IEnumerable<TValue> Values => valueEnumerable ??= new ValueEnumerable(this);

		public NewAssetDictionary() : this(4) { }

		public NewAssetDictionary(int capacity)
		{
			if(capacity == 0)
			{
				keys = Array.Empty<TKey>();
				values = Array.Empty<TValue>();
			}
			else
			{
				keys = new TKey[capacity];
				values = new TValue[capacity];
			}
		}

		public void Add(TKey key, TValue value)
		{
			if(Count == Capacity)
				Reallocate();
			keys[Count] = key;
			values[Count] = value;
			Count++;
		}

		public void Add(KeyValuePair<TKey,TValue> pair)
		{
			Add(pair.Key, pair.Value);
		}

		public void AddNew()
		{
			TKey key = new();
			TValue value = new();
			Add(key, value);
		}

		public TKey GetKey(int index)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			return keys[index];
		}

		public void SetKey(int index, TKey newKey)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			keys[index] = newKey;
		}

		public TValue GetValue(int index)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			return values[index];
		}

		public void SetValue(int index, TValue newValue)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			values[index] = newValue;
		}

		public KeyValuePair<TKey, TValue> this[int index]
		{
			get
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				return new KeyValuePair<TKey, TValue>(keys[index], values[index]);
			}
			set
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				keys[index] = value.Key;
				values[index] = value.Value;
			}
		}

		private void Reallocate()
		{
			int newSize = GetNewSize();
			TKey[] newKeys = new TKey[newSize];
			TValue[] newValues = new TValue[newSize];
			Array.Copy(keys, 0, newKeys, 0, keys.Length);
			Array.Copy(values, 0, values, 0, values.Length);
			keys = newKeys;
			values = newValues;
		}

		private int GetNewSize()
		{
			int newSize = (int)System.Math.Min(2 * (long)Capacity, int.MaxValue);
			return System.Math.Max(newSize, 1);
		}

		private class KeyEnumerable : IEnumerable<TKey>
		{
			private readonly NewAssetDictionary<TKey, TValue> dictionary;

			public KeyEnumerable(NewAssetDictionary<TKey, TValue> dictionary)
			{
				this.dictionary = dictionary;
			}

			public IEnumerator<TKey> GetEnumerator()
			{
				for (int i = 0; i < dictionary.Count; i++)
				{
					yield return dictionary.keys[i];
				}
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		private class ValueEnumerable : IEnumerable<TValue>
		{
			private readonly NewAssetDictionary<TKey, TValue> dictionary;

			public ValueEnumerable(NewAssetDictionary<TKey, TValue> dictionary)
			{
				this.dictionary = dictionary;
			}

			public IEnumerator<TValue> GetEnumerator()
			{
				for (int i = 0; i < dictionary.Count; i++)
				{
					yield return dictionary.values[i];
				}
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}
