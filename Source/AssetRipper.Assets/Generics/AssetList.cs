using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AssetRipper.Assets.Generics;

public sealed class AssetList<T> : AccessListBase<T>
	where T : notnull, new()
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private const int DefaultCapacity = 4;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int count = 0;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private T[] items;

	public AssetList()
	{
		items = [];
	}

	public AssetList(int capacity)
	{
		items = capacity == 0 ? [] : new T[capacity];
	}

	/// <inheritdoc/>
	public override int Count => count;

	/// <inheritdoc/>
	public override int Capacity
	{
		get => items.Length;
		set
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(value, count);

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
					items = [];
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
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			return items[index];
		}
		set
		{
			ThrowIfElementsNotImmutable();

			if ((uint)index >= (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			items[index] = value;
		}
	}

	/// <inheritdoc/>
	public override void Add(T item)
	{
		ThrowIfElementsNotImmutable();
		AddInternal(item);
	}

	private void AddInternal(T item)
	{
		if (count == Capacity)
		{
			Grow(count + 1);
		}

		items[count] = item;
		count++;
	}

	/// <inheritdoc/>
	public override T AddNew()
	{
		T newItem = new();
		AddInternal(newItem);
		return newItem;
	}

	public void AddRange(IEnumerable<T> enumerable)
	{
		ThrowIfElementsNotImmutable();
		if (enumerable is IReadOnlyCollection<T> collection)
		{
			EnsureCapacity(count + collection.Count);
			switch (collection)
			{
				case T[] array:
					array.AsSpan().CopyTo(items.AsSpan(count, array.Length));
					count += array.Length;
					break;
				case IReadOnlyList<T> list:
					for (int i = 0; i < list.Count; i++)
					{
						items[count + i] = list[i];
					}
					count += list.Count;
					break;
				default:
					foreach (T item in enumerable)
					{
						items[count] = item;
						count++;
					}
					break;
			}
		}
		else
		{
			foreach (T item in enumerable)
			{
				AddInternal(item);
			}
		}
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
		ArgumentNullException.ThrowIfNull(array);

		if (arrayIndex < 0 || arrayIndex > array.Length - count)
		{
			throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, null);
		}

		Array.Copy(items, 0, array, arrayIndex, count);
	}

	public void CopyTo(Span<T> destination)
	{
		new ReadOnlySpan<T>(items, 0, count).CopyTo(destination);
	}

	/// <summary>
	/// Get a span for this list.
	/// </summary>
	/// <remarks>
	/// <typeparamref name="T"/> must be blittable.
	/// </remarks>
	/// <returns>A span for the underlying array, with length equal to <see cref="Count"/>.</returns>
	public Span<T> GetSpan()
	{
		if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
		{
			return new(items, 0, count);
		}
		else
		{
			throw new NotSupportedException("Type must be blittable.");
		}
	}

	/// <inheritdoc/>
	public override int IndexOf(T item) => Array.IndexOf(items, item, 0, count);

	/// <inheritdoc/>
	public override void Insert(int index, T item)
	{
		ThrowIfElementsNotImmutable();

		// Note that insertions at the end are legal.
		if ((uint)index > (uint)count)
		{
			throw new ArgumentOutOfRangeException(nameof(index));
		}

		if (count == items.Length)
		{
			Grow(count + 1);
		}

		if (index < count)
		{
			Array.Copy(items, index, items, index + 1, count - index);
		}

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
		{
			throw new ArgumentOutOfRangeException(nameof(index));
		}

		count--;
		if (index < count)
		{
			Array.Copy(items, index + 1, items, index, count - index);
		}
		items[count] = default!;
	}

	/// <summary>
	/// Ensures that the capacity of this list is at least the specified <paramref name="capacity"/>.
	/// If the current capacity of the list is less than specified <paramref name="capacity"/>,
	/// the capacity is increased by continuously twice current capacity until it is at least the specified <paramref name="capacity"/>.
	/// </summary>
	/// <param name="capacity">The minimum capacity to ensure.</param>
	/// <returns>The new capacity of this list.</returns>
	public override int EnsureCapacity(int capacity)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);
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

	private static void ThrowIfElementsNotImmutable()
	{
		if (!typeof(T).IsValueType && typeof(T) != typeof(string) && typeof(T) != typeof(Utf8String))
		{
			throw new NotSupportedException($"Only immutable elements can be used in {nameof(Add)}, {nameof(Insert)}, and the setter for this[int].");
		}
	}
}
