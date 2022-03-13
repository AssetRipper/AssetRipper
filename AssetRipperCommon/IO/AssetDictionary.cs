using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.IO
{
	/// <summary>
	/// A dictionary class supporting non-unique keys
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
	public sealed class AssetDictionary<TKey, TValue> : AccessDictionaryBase<TKey, TValue>, IDependent
		//where TKey : new() 
		//where TValue : new()
	{
		private static readonly bool isDependentType = NullableKeyValuePair<TKey, TValue>.IsDependentType;
		private const int DefaultCapacity = 4;
		private NullableKeyValuePair<TKey, TValue>[] pairs;
		private int count = 0;

		public AssetDictionary() : this(DefaultCapacity) { }

		public AssetDictionary(int capacity)
		{
			pairs = capacity == 0 ? Array.Empty<NullableKeyValuePair<TKey, TValue>>() : new NullableKeyValuePair<TKey, TValue>[capacity];
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			if (isDependentType)
			{
				foreach (NullableKeyValuePair<TKey, TValue> keyValuePair in this)
				{
					if (keyValuePair != null)
					{
						foreach (PPtr<IUnityObjectBase> dependency in keyValuePair.FetchDependencies(context))
						{
							yield return dependency;
						}
					}
				}
			}
		}

		/// <inheritdoc/>
		public override int Count => count;

		/// <inheritdoc/>
		public override int Capacity
		{
			get => pairs.Length;
			set
			{
				if (value < count)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				if (value != pairs.Length)
				{
					if (value > 0)
					{
						NullableKeyValuePair<TKey, TValue>[] newPairs = new NullableKeyValuePair<TKey, TValue>[value];
						if (count > 0)
						{
							Array.Copy(pairs, newPairs, count);
						}
						pairs = newPairs;
					}
					else
					{
						pairs = Array.Empty<NullableKeyValuePair<TKey, TValue>>();
					}
				}
			}
		}

		/// <inheritdoc/>
		public override void Add(TKey key, TValue value)
		{
			Add(new NullableKeyValuePair<TKey, TValue>(key, value));
		}

		/// <inheritdoc/>
		public override void Add(NullableKeyValuePair<TKey,TValue> pair)
		{
			if (count == Capacity)
				Grow(count + 1);
			pairs[count] = pair;
			count++;
		}

		/// <inheritdoc/>
		public override void AddNew() { } //=> Add(new TKey(), new TValue());

		/// <inheritdoc/>
		public override TKey GetKey(int index)
		{
			if ((uint)index >= (uint)count)
				throw new ArgumentOutOfRangeException(nameof(index));

			return pairs[index].Key;
		}

		/// <inheritdoc/>
		public override void SetKey(int index, TKey newKey)
		{
			if ((uint)index >= (uint)count)
				throw new ArgumentOutOfRangeException(nameof(index));

			pairs[index] = new NullableKeyValuePair<TKey, TValue>(newKey, pairs[index].Value);
		}

		/// <inheritdoc/>
		public override TValue GetValue(int index)
		{
			if (index < 0 || index >= count)
				throw new ArgumentOutOfRangeException(nameof(index));

			return pairs[index].Value;
		}

		/// <inheritdoc/>
		public override void SetValue(int index, TValue newValue)
		{
			if ((uint)index >= (uint)count)
				throw new ArgumentOutOfRangeException(nameof(index));

			pairs[index] = new KeyValuePair<TKey, TValue>(pairs[index].Key, newValue);
		}

		/// <inheritdoc/>
		public override NullableKeyValuePair<TKey, TValue> this[int index]
		{
			get
			{
				if ((uint)index >= (uint)count)
					throw new ArgumentOutOfRangeException(nameof(index));

				return pairs[index];
			}
			set
			{
				if (index < 0 || index >= count)
					throw new ArgumentOutOfRangeException(nameof(index));

				pairs[index] = value;
			}
		}

		/// <inheritdoc/>
		public override int IndexOf(NullableKeyValuePair<TKey, TValue> item) => Array.IndexOf(pairs, item, 0, count);

		/// <inheritdoc/>
		public override void Insert(int index, NullableKeyValuePair<TKey, TValue> item)
		{
			// Note that insertions at the end are legal.
			if ((uint)index > (uint)count)
				throw new ArgumentOutOfRangeException(nameof(index));

			if (count == pairs.Length)
				Grow(count + 1);

			if (index < count)
				Array.Copy(pairs, index, pairs, index + 1, count - index);

			pairs[index] = item;
			count++;
		}

		/// <inheritdoc/>
		public override void RemoveAt(int index)
		{
			if ((uint)index >= (uint)count)
				throw new ArgumentOutOfRangeException(nameof(index));

			count--;
			if (index < count)
			{
				Array.Copy(pairs, index + 1, pairs, index, count - index);
			}
			pairs[count] = default;
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

		/// <inheritdoc/>
		public override bool Contains(NullableKeyValuePair<TKey, TValue> item)
		{
			return IndexOf(item) >= 0;
		}

		/// <inheritdoc/>
		public override void CopyTo(NullableKeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			if(array == null)
				throw new ArgumentNullException(nameof(array));

			if (arrayIndex < 0 || arrayIndex >= array.Length - count)
				throw new ArgumentOutOfRangeException(nameof(arrayIndex));

			Array.Copy(pairs, 0, array, arrayIndex, count);
		}

		/// <inheritdoc/>
		public override bool Remove(NullableKeyValuePair<TKey, TValue> item)
		{
			int index = IndexOf(item);
			if (index >= 0)
			{
				RemoveAt(index);
				return true;
			}
			return false;
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
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(capacity));
			}
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
				newcapacity = Array.MaxLength;

			// If the computed capacity is still less than specified, set to the original argument.
			// Capacities exceeding Array.MaxLength will be surfaced as OutOfMemoryException by Array.Resize.
			if (newcapacity < capacity)
				newcapacity = capacity;

			Capacity = (int)newcapacity;
		}
	}
}
