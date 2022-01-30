using System;
using System.Collections.Generic;

namespace AssetRipper.Core.IO.DictionaryTest
{
	public class NewAssetDictionary<TKey, TValue> : IAccessDictionary<TKey, TValue>
		where TKey : new() 
		where TValue : new()
	{
		private KeyValuePair<TKey, TValue>[] pairs;

		public int Count { get; private set; } = 0;

		public int Capacity => pairs.Length;

		private KeyEnumerable keyEnumerable;
		public IEnumerable<TKey> Keys => keyEnumerable ??= new KeyEnumerable(this);

		private ValueEnumerable valueEnumerable;
		public IEnumerable<TValue> Values => valueEnumerable ??= new ValueEnumerable(this);

		public NewAssetDictionary() : this(4) { }

		public NewAssetDictionary(int capacity)
		{
			if(capacity == 0)
			{
				pairs = Array.Empty<KeyValuePair<TKey, TValue>>();
			}
			else
			{
				pairs = new KeyValuePair<TKey, TValue>[capacity];
			}
		}

		public void Add(TKey key, TValue value)
		{
			Add(new KeyValuePair<TKey, TValue>(key, value));
		}

		public void Add(KeyValuePair<TKey,TValue> pair)
		{
			if (Count == Capacity)
				Reallocate();
			pairs[Count] = pair;
			Count++;
		}

		public void AddNew() => Add(new TKey(), new TValue());

		public TKey GetKey(int index)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			return pairs[index].Key;
		}

		public void SetKey(int index, TKey newKey)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			pairs[index] = new KeyValuePair<TKey, TValue>(newKey, pairs[index].Value);
		}

		public TValue GetValue(int index)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			return pairs[index].Value;
		}

		public void SetValue(int index, TValue newValue)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			pairs[index] = new KeyValuePair<TKey, TValue>(pairs[index].Key, newValue);
		}

		public KeyValuePair<TKey, TValue> this[int index]
		{
			get
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				return pairs[index];
			}
			set
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				pairs[index] = value;
			}
		}

		private void Reallocate()
		{
			int newSize = GetNewSize();
			KeyValuePair<TKey, TValue>[] newPairs = new KeyValuePair<TKey, TValue>[newSize];
			Array.Copy(pairs, 0, newPairs, 0, pairs.Length);
			pairs = newPairs;
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
					yield return dictionary.pairs[i].Key;
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
					yield return dictionary.pairs[i].Value;
				}
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}
