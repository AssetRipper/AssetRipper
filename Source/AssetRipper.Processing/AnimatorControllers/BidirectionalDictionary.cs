using System.Collections;

namespace AssetRipper.Processing.AnimatorControllers;

/// <summary>
/// Bidirectional Dictionary, valid for 2 different Types T1 and T2
/// </summary>
public sealed class BidirectionalDictionary<T1, T2> : IDictionary<T1, T2>
	where T1 : notnull
	where T2 : notnull
{
	private readonly Dictionary<T1, T2> forward = new();
	private readonly Dictionary<T2, T1> backward = new();

	public int Count { get { return forward.Count; } }

	public bool IsReadOnly { get { return false; } }

	public T2 this[T1 item1]
	{
		get
		{
			return forward[item1];
		}
		set
		{
			forward[item1] = value;
			backward[value] = item1;
		}
	}

	public T1 this[T2 item2]
	{
		get
		{
			return backward[item2];
		}
		set
		{
			forward[value] = item2;
			backward[item2] = value;
		}
	}

	public ICollection<T1> Keys { get { return forward.Keys; } }

	public ICollection<T2> Values { get { return backward.Keys; } }

	public void Add(T1 item1, T2 item2)
	{
		ArgumentNullException.ThrowIfNull(item1);
		ArgumentNullException.ThrowIfNull(item2);
		if (forward.ContainsKey(item1))
		{
			throw new ArgumentException("An element with the same key already exists.", nameof(item1));
		}
		if (backward.ContainsKey(item2))
		{
			throw new ArgumentException("An element with the same key already exists.", nameof(item2));
		}
		forward.Add(item1, item2);
		backward.Add(item2, item1);
	}

	public void Add(KeyValuePair<T1, T2> item1_item2)
	{
		Add(item1_item2.Key, item1_item2.Value);
	}

	public void Add(KeyValuePair<T2, T1> item2_item1)
	{
		Add(item2_item1.Value, item2_item1.Key);
	}

	public void Clear()
	{
		forward.Clear();
		backward.Clear();
	}

	public bool Contains(KeyValuePair<T1, T2> item1_item2)
	{
		return forward.Contains(item1_item2);
	}

	public bool Contains(KeyValuePair<T2, T1> item2_item1)
	{
		return backward.Contains(item2_item1);
	}

	public bool ContainsKey(T1 item1)
	{
		return forward.ContainsKey(item1);
	}

	public bool ContainsKey(T2 item2)
	{
		return backward.ContainsKey(item2);
	}

	public void CopyTo(KeyValuePair<T1, T2>[] array, int arrayIndex)
	{
		ArgumentNullException.ThrowIfNull(array);
		ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(forward.Count, array.Length - arrayIndex);

		foreach (KeyValuePair<T1, T2> kvp in forward)
		{
			array[arrayIndex] = kvp;
			arrayIndex++;
		}
	}

	public void CopyTo(KeyValuePair<T2, T1>[] array, int arrayIndex)
	{
		ArgumentNullException.ThrowIfNull(array);
		ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(backward.Count, array.Length - arrayIndex);

		foreach (KeyValuePair<T2, T1> kvp in backward)
		{
			array[arrayIndex] = kvp;
			arrayIndex++;
		}
	}

	public IEnumerator GetEnumerator()
	{
		return forward.GetEnumerator();
	}

	IEnumerator<KeyValuePair<T1, T2>> IEnumerable<KeyValuePair<T1, T2>>.GetEnumerator()
	{
		return forward.GetEnumerator();
	}

	public bool Remove(T1 item1)
	{
		if (forward.TryGetValue(item1, out T2? item2))
		{
			backward.Remove(item2);
			forward.Remove(item1);
			return true;
		}
		return false;
	}

	public bool Remove(T2 item2)
	{
		if (backward.TryGetValue(item2, out T1? item1))
		{
			forward.Remove(item1);
			backward.Remove(item2);
			return true;
		}
		return false;
	}

	public bool Remove(KeyValuePair<T1, T2> item1_item2)
	{
		if (forward.Contains(item1_item2))
		{
			forward.Remove(item1_item2.Key);
			backward.Remove(item1_item2.Value);
			return true;
		}
		return false;
	}

	public bool Remove(KeyValuePair<T2, T1> item2_item1)
	{
		if (backward.Contains(item2_item1))
		{
			forward.Remove(item2_item1.Value);
			backward.Remove(item2_item1.Key);
			return true;
		}
		return false;
	}

	public bool TryGetValue(T1 item1, [MaybeNullWhen(false)] out T2 item2)
	{
		return forward.TryGetValue(item1, out item2);
	}

	public bool TryGetValue(T2 item2, [MaybeNullWhen(false)] out T1 item1)
	{
		return backward.TryGetValue(item2, out item1);
	}
}
