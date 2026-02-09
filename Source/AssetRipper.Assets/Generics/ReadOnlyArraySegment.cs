using System.Collections;

namespace AssetRipper.Assets.Generics;

public readonly struct ReadOnlyArraySegment<T> : IReadOnlyList<T>
{
	// ReadOnlyArraySegment<T> doesn't implement IEquatable<T>, even though it provides a strongly-typed
	// Equals(T), as that results in different comparison semantics than comparing item-by-item
	// the elements returned from its IEnumerable<T> implementation.  This then is a breaking change
	// for usage like that in xunit's Assert.Equal, which will prioritize using an instance's IEquatable<T>
	// over its IEnumerable<T>.

	public static ReadOnlyArraySegment<T> Empty { get; } = new ReadOnlyArraySegment<T>(Array.Empty<T>());

	private readonly T[]? _array;
	private readonly int _offset;

	public ReadOnlyArraySegment(T[] array)
	{
		ArgumentNullException.ThrowIfNull(array, nameof(array));

		_array = array;
		_offset = 0;
		Count = array.Length;
	}

	public ReadOnlyArraySegment(T[] array, int offset, int count)
	{
		ArgumentNullException.ThrowIfNull(array, nameof(array));
		ValidateOffsetAndCount(array, offset, count);

		_array = array;
		_offset = offset;
		Count = count;
	}

	public int Count { get; }

	public T this[int index]
	{
		get
		{
			ThrowInvalidOperationIfDefault();
			ThrowIfIndexNegativeOrGreaterEqualThanCount(index);

			return _array[_offset + index];
		}
	}

	public ArraySegment<T>.Enumerator GetEnumerator()
	{
		ThrowInvalidOperationIfDefault();
		return new ArraySegment<T>(_array, _offset, Count).GetEnumerator();
	}

	public override int GetHashCode()
	{
		return _array is null ? 0 : HashCode.Combine(_offset, Count, _array);
	}

	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		return obj is ReadOnlyArraySegment<T> other && Equals(other);
	}

	public bool Equals(ReadOnlyArraySegment<T> obj)
	{
		return obj._array == _array && obj._offset == _offset && obj.Count == Count;
	}

	public ReadOnlyArraySegment<T> Slice(int index)
	{
		ThrowInvalidOperationIfDefault();
		ThrowIfIndexNegativeOrGreaterThanCount(index);

		return new ReadOnlyArraySegment<T>(_array, _offset + index, Count - index);
	}

	public ReadOnlyArraySegment<T> Slice(int index, int count)
	{
		ThrowInvalidOperationIfDefault();

		if ((uint)index > (uint)Count || (uint)count > (uint)(Count - index))
		{
			throw new ArgumentOutOfRangeException(nameof(index));
		}

		return new ReadOnlyArraySegment<T>(_array, _offset + index, count);
	}

	public T[] ToArray()
	{
		if (Count == 0)
		{
			return Array.Empty<T>();
		}

		ThrowInvalidOperationIfDefault();

		T[] array = new T[Count];
		Array.Copy(_array, _offset, array, 0, Count);
		return array;
	}

	public static bool operator ==(ReadOnlyArraySegment<T> a, ReadOnlyArraySegment<T> b) => a.Equals(b);
	public static bool operator !=(ReadOnlyArraySegment<T> a, ReadOnlyArraySegment<T> b) => !(a == b);

	public static implicit operator ReadOnlyArraySegment<T>(T[]? array)
	{
		return array is not null ? new ReadOnlyArraySegment<T>(array) : default;
	}

	public static implicit operator ReadOnlyArraySegment<T>(ArraySegment<T> segment)
	{
		return segment.Array is not null ? new ReadOnlyArraySegment<T>(segment.Array, segment.Offset, segment.Count) : default;
	}

	public static implicit operator ReadOnlySpan<T>(ReadOnlyArraySegment<T> segment)
	{
		return new ReadOnlySpan<T>(segment._array, segment._offset, segment.Count);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	private static void ValidateOffsetAndCount(T[] array, int offset, int count)
	{
		if ((uint)offset > (uint)array.Length || (uint)count > (uint)(array.Length - offset))
		{
			throw new ArgumentException("Invalid offset and count.");
		}
	}

	private void ThrowIfIndexNegativeOrGreaterThanCount(int index)
	{
		if ((uint)index > (uint)Count)
		{
			throw new ArgumentOutOfRangeException(nameof(index));
		}
	}

	private void ThrowIfIndexNegativeOrGreaterEqualThanCount(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException(nameof(index));
		}
	}

	[MemberNotNull(nameof(_array))]
	private void ThrowInvalidOperationIfDefault()
	{
		if (_array == null)
		{
			throw new InvalidOperationException("Array was null.");
		}
	}
}
