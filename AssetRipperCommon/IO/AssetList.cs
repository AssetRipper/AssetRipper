using System;

namespace AssetRipper.Core.IO
{
	public sealed class AssetList<T> : AccessListBase<T> where T : new()
	{
		private const int DefaultCapacity = 4;
		private int count = 0;
		private T[] items;

		public AssetList() : this(DefaultCapacity) { }

		public AssetList(int capacity)
		{
			items = capacity == 0 ? Array.Empty<T>() : new T[capacity];
		}

		/// <inheritdoc/>
		public override int Count => count;

		/// <inheritdoc/>
		public override int Capacity
		{
			get => items.Length;
			set
			{
				if (value < count)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				if (value != items.Length)
				{
					if (value > 0)
					{
						T[] newElements = new T[value];
						if (count > 0)
						{
							Array.Copy(items, newElements, count);
						}
						items = newElements;
					}
					else
					{
						items = Array.Empty<T>();
					}
				}
			}
		}

		/// <inheritdoc/>
		public override T this[int index]
		{
			get
			{
				if ((uint)index >= (uint)count)
					throw new ArgumentOutOfRangeException(nameof(index));

				return items[index];
			}
			set
			{
				if ((uint)index >= (uint)count)
					throw new ArgumentOutOfRangeException(nameof(index));

				items[index] = value;
			}
		}

		/// <inheritdoc/>
		public override void Add(T item)
		{
			if (count == Capacity)
				Grow(count + 1);
			items[count] = item;
			count++;
		}

		/// <inheritdoc/>
		public override T AddNew()
		{
			T newItem = new();
			Add(newItem);
			return newItem;
		}

		/// <inheritdoc/>
		public override void Clear()
		{
			if (count > 0)
			{
				Array.Clear(items, 0, count); // Clear the elements so that the gc can reclaim the references.
			}
			count = 0;
		}

		/// <inheritdoc/>
		public override bool Contains(T item) => IndexOf(item) >= 0;

		/// <inheritdoc/>
		public override void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			if (arrayIndex < 0 || arrayIndex >= array.Length - count)
				throw new ArgumentOutOfRangeException(nameof(arrayIndex));

			Array.Copy(items, 0, array, arrayIndex, count);
		}

		/// <inheritdoc/>
		public override int IndexOf(T item) => Array.IndexOf(items, item, 0, count);

		/// <inheritdoc/>
		public override void Insert(int index, T item)
		{
			// Note that insertions at the end are legal.
			if ((uint)index > (uint)count)
				throw new ArgumentOutOfRangeException(nameof(index));

			if (count == items.Length) 
				Grow(count + 1);

			if (index < count)
				Array.Copy(items, index, items, index + 1, count - index);

			items[index] = item;
			count++;
		}

		/// <inheritdoc/>
		public override bool Remove(T item)
		{
			int index = IndexOf(item);
			if (index >= 0)
			{
				RemoveAt(index);
				return true;
			}
			return false;
		}

		/// <inheritdoc/>
		public override void RemoveAt(int index)
		{
			if ((uint)index >= (uint)count)
				throw new ArgumentOutOfRangeException(nameof(index));

			count--;
			if (index < count)
			{
				Array.Copy(items, index + 1, items, index, count - index);
			}
			items[count] = default;
		}

		public Span<T> AsSpan() => items.AsSpan(0, count);

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
			if (items.Length < capacity)
			{
				Grow(capacity);
			}

			return items.Length;
		}

		private void Grow(int capacity)
		{
			long newcapacity = items.Length == 0 ? DefaultCapacity : 2L * items.Length;

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
