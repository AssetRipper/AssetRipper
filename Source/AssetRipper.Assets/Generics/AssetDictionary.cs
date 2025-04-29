namespace AssetRipper.Assets.Generics
{
	/// <summary>
	/// A dictionary class supporting non-unique keys
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
	public sealed class AssetDictionary<TKey, TValue> : AccessDictionaryBase<TKey, TValue>
		where TKey : notnull, new()
		where TValue : notnull, new()
	{
		private const int DefaultCapacity = 4;
		private AssetPair<TKey, TValue>[] pairs;
		private int count = 0;

		public AssetDictionary()
		{
			pairs = Array.Empty<AssetPair<TKey, TValue>>();
		}

		public AssetDictionary(int capacity)
		{
			pairs = capacity == 0 ? Array.Empty<AssetPair<TKey, TValue>>() : new AssetPair<TKey, TValue>[capacity];
		}

		/// <inheritdoc/>
		public override int Count => count;

		/// <inheritdoc/>
		public override int Capacity
		{
			get => pairs.Length;
			set
			{
				ArgumentOutOfRangeException.ThrowIfLessThan(value, count);

				if (value != pairs.Length)
				{
					if (value > 0)
					{
						AssetPair<TKey, TValue>[] newPairs = new AssetPair<TKey, TValue>[value];
						if (count > 0)
						{
							Array.Copy(pairs, newPairs, count);
						}
						pairs = newPairs;
					}
					else
					{
						pairs = Array.Empty<AssetPair<TKey, TValue>>();
					}
				}
			}
		}

		/// <inheritdoc/>
		public override void Add(TKey key, TValue value)
		{
			AssetPair<TKey, TValue> pair = AddNew();
			pair.Key = key;
			pair.Value = value;
		}

		/// <inheritdoc/>
		private void Add(AssetPair<TKey, TValue> pair)
		{
			if (count == Capacity)
			{
				Grow(count + 1);
			}

			pairs[count] = pair;
			count++;
		}

		/// <inheritdoc/>
		public override AssetPair<TKey, TValue> AddNew()
		{
			AssetPair<TKey, TValue> pair = new();
			Add(pair);
			return pair;
		}

		/// <inheritdoc/>
		public override TKey GetKey(int index)
		{
			if ((uint)index >= (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			return pairs[index].Key;
		}

		/// <inheritdoc/>
		public override void SetKey(int index, TKey newKey)
		{
			if ((uint)index >= (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			pairs[index].Key = newKey;
		}

		/// <inheritdoc/>
		public override TValue GetValue(int index)
		{
			if (index < 0 || index >= count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			return pairs[index].Value;
		}

		/// <inheritdoc/>
		public override void SetValue(int index, TValue newValue)
		{
			if ((uint)index >= (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			pairs[index].Value = newValue;
		}

		public override AssetPair<TKey, TValue> GetPair(int index)
		{
			if ((uint)index >= (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			return pairs[index];
		}

		/// <inheritdoc/>
		public override void RemoveAt(int index)
		{
			if ((uint)index >= (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			count--;
			if (index < count)
			{
				Array.Copy(pairs, index + 1, pairs, index, count - index);
			}
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
			pairs[count] = default;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
		}

		/// <inheritdoc/>
		public override void Clear()
		{
			if (count > 0)
			{
				Array.Clear(pairs, 0, count); // Clear the elements so that the gc can reclaim the references.
			}
			count = 0;
		}

		public override bool TryGetSinglePairForKey(TKey key, [NotNullWhen(true)] out AccessPairBase<TKey, TValue>? pair)
		{
			if (key is null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			int hash = key.GetHashCode();
			bool found = false;
			pair = null;
			for (int i = Count - 1; i > -1; i--)
			{
				AssetPair<TKey, TValue> p = pairs[i];
				if (p.Key.GetHashCode() == hash && key.Equals(p.Key))
				{
					if (found)
					{
						throw new Exception("Found more than one matching key");
					}
					else
					{
						found = true;
						pair = p;
					}
				}
			}
			return found;
		}

		/// <summary>
		/// Ensures that the capacity of this list is at least the specified <paramref name="capacity"/>.
		/// If the current capacity of the list is less than specified <paramref name="capacity"/>,
		/// the capacity is increased by continuously twice current capacity until it is at least the specified <paramref name="capacity"/>.
		/// </summary>
		/// <param name="capacity">The minimum capacity to ensure.</param>
		/// <returns>The new capacity of this list.</returns>
		public int EnsureCapacity(int capacity)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(capacity);
			if (pairs.Length < capacity)
			{
				Grow(capacity);
			}

			return pairs.Length;
		}

		private void Grow(int capacity)
		{
			long newcapacity = pairs.Length == 0 ? DefaultCapacity : 2L * pairs.Length;

			// Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
			// Note that this check works even when _items.Length overflowed thanks to the (uint) cast
			if (newcapacity > Array.MaxLength)
			{
				newcapacity = Array.MaxLength;
			}

			// If the computed capacity is still less than specified, set to the original argument.
			// Capacities exceeding Array.MaxLength will be surfaced as OutOfMemoryException by Array.Resize.
			if (newcapacity < capacity)
			{
				newcapacity = capacity;
			}

			Capacity = (int)newcapacity;
		}

		/// <inheritdoc/>
		public override IEnumerator<AssetPair<TKey, TValue>> GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
			{
				yield return pairs[i];
			}
		}
	}
}
