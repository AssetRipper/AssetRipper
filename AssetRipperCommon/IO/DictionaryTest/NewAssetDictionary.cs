using System;
using System.Collections.Generic;

namespace AssetRipper.Core.IO.DictionaryTest
{
	public partial class NewAssetDictionary<TKey, TValue> : IAccessDictionary<TKey, TValue>
		where TKey : new() 
		where TValue : new()
	{
		private TKey[] keys;
		private TValue[] values;

		public int Count { get; private set; } = 0;

		public int Capacity => keys.Length;

		private KeyEnumerable keyEnumerable;
		public IEnumerable<TKey> Keys => keyEnumerable ??= new KeyEnumerable(this);

		public IEnumerable<TValue> Values
		{
			get
			{
				for (int i = 0; i < Count; i++)
				{
					yield return values[i];
				}
			}
		}

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

		public Span<TKey> GetKeySpan()
		{
			return new Span<TKey>(keys, 0, Count);
		}

		public Span<TValue> GetValueSpan()
		{
			return new Span<TValue>(values, 0, Count);
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
	}
}
