using AssetRipper.Numerics;
using AssetRipper.Primitives;
using System.Collections;
using System.Text.Json.Serialization;

namespace AssetRipper.AssemblyDumper.Utils;

[JsonConverter(typeof(VersionedListConverterFactory))]
public sealed class VersionedList<T> : IList<KeyValuePair<UnityVersion, T?>>
{
	private readonly List<KeyValuePair<UnityVersion, T?>> _list = new();
	private readonly Func<T?, T?> cloneFactory;

	public VersionedList()
	{
		if (typeof(T).IsAssignableTo(typeof(IDeepCloneable<T>)))
		{
			cloneFactory = (original) =>
			{
				//boxing could happen here
				return original is null ? default : ((IDeepCloneable<T>)original).DeepClone();
			};
		}
		else if (typeof(T).IsAssignableTo(typeof(ValueType)) || typeof(T) == typeof(string))
		{
			cloneFactory = (original) => original;
		}
		else
		{
			cloneFactory = (_) => throw new NotSupportedException();
		}
	}

	public VersionedList(Func<T?, T?> cloneFactory)
	{
		this.cloneFactory = cloneFactory;
	}

	public KeyValuePair<UnityVersion, T?> this[int index]
	{
		get => _list[index];
		set
		{
			if (index > 0 && value.Key <= this[index - 1].Key)
			{
				throw new ArgumentException(null, nameof(value));
			}
			if (index < Count - 1 && value.Key >= this[index + 1].Key)
			{
				throw new ArgumentException(null, nameof(value));
			}
			_list[index] = value;
		}
	}

	public int Count => _list.Count;

	public int Capacity { get => _list.Capacity; set => _list.Capacity = value; }

	public bool IsReadOnly => false;

	/// <summary>
	/// Throws if <see cref="Count"/> == 0
	/// </summary>
	private UnityVersion MostRecentVersion => this[^1].Key;

	public void Add(KeyValuePair<UnityVersion, T?> pair)
	{
		if (Count > 0 && pair.Key <= MostRecentVersion)
		{
			throw new Exception($"Version {pair.Key} was not greater than the most recent version {MostRecentVersion}");
		}
		else
		{
			_list.Add(pair);
		}
	}

	public void Add(UnityVersion version, T? item) => Add(new KeyValuePair<UnityVersion, T?>(version, item));

	public void Clear() => _list.Clear();

	public bool Contains(KeyValuePair<UnityVersion, T?> item) => _list.Contains(item);

	public void CopyTo(KeyValuePair<UnityVersion, T?>[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

	public T? GetItemForVersion(UnityVersion version)
	{
		if (Count == 0 || this[0].Key > version)
		{
			return default;
		}

		for (int i = 0; i < Count - 1; i++)
		{
			if (this[i].Key <= version && version < this[i + 1].Key)
			{
				return this[i].Value;
			}
		}

		return this[Count - 1].Value;
	}

	public T? GetLastValue()
	{
		return Count != 0 ? _list[Count - 1].Value : throw new InvalidOperationException();
	}

	public Range<UnityVersion> GetRange(int index)
	{
		return index == Count - 1
			? new Range<UnityVersion>(this[index].Key, UnityVersion.MaxVersion)
			: new Range<UnityVersion>(this[index].Key, this[index + 1].Key);
	}

	public Range<UnityVersion> GetRangeForItem(T item)
	{
		for (int i = 0; i < Count; i++)
		{
			if (EqualityComparer<T>.Default.Equals(this[i].Value, item))
			{
				return GetRange(i);
			}
		}

		throw new Exception($"Item not found: {item}");
	}

	public int IndexOf(KeyValuePair<UnityVersion, T?> item) => _list.IndexOf(item);

	public void Insert(int index, KeyValuePair<UnityVersion, T?> item)
	{
		if (index == Count)
		{
			Add(item);
		}
		else
		{
			throw new NotSupportedException();
		}
	}

	public bool Remove(KeyValuePair<UnityVersion, T?> item)
	{
		if (Count > 0 && EqualityComparer<KeyValuePair<UnityVersion, T?>>.Default.Equals(item, this[^1]))
		{
			_list.RemoveAt(Count - 1);
			return true;
		}
		else
		{
			return false;
		}
	}

	public void RemoveAt(int index)
	{
		if (index == Count - 1)
		{
			Pop();
		}
		else
		{
			throw new NotSupportedException();
		}
	}

	public void Pop()
	{
		if (Count > 0)
		{
			_list.RemoveAt(Count - 1);
		}
		else
		{
			throw new InvalidOperationException();
		}
	}

	public void Divide(UnityVersion divisionPoint)
	{
		if (Count == 0)
		{
			throw new InvalidOperationException();
		}

		if (divisionPoint < this[0].Key)
		{
			throw new ArgumentOutOfRangeException(nameof(divisionPoint), divisionPoint, null);
		}

		int insertionIndex = -1;
		T? clone = default;

		for (int i = 0; i < Count - 1; i++)
		{
			KeyValuePair<UnityVersion, T?> currentPair = this[i];
			if (currentPair.Key <= divisionPoint && divisionPoint < this[i + 1].Key)
			{
				if (currentPair.Key == divisionPoint)
				{
					return;
				}
				else
				{
					insertionIndex = i + 1;
					clone = currentPair.Value is null ? default : cloneFactory(currentPair.Value);
					break;
				}
			}
		}

		if (insertionIndex < 0)
		{
			insertionIndex = Count;
			KeyValuePair<UnityVersion, T?> lastPair = this[Count - 1];
			if (lastPair.Key == divisionPoint)
			{
				return;
			}
			clone = lastPair.Value is null ? default : cloneFactory(lastPair.Value);
		}

		_list.Insert(insertionIndex, new KeyValuePair<UnityVersion, T?>(divisionPoint, clone));
	}

	public VersionedList<T> GetSubList(Range<UnityVersion> range)
	{
		VersionedList<T> result = new();
		for (int i = 0; i < Count; i++)
		{
			Range<UnityVersion> currentRange = GetRange(i);
			if (currentRange.Intersects(range, out Range<UnityVersion> intersection))
			{
				result.Add(intersection.Start, this[i].Value);
			}
		}
		return result;
	}

	public IEnumerator<KeyValuePair<UnityVersion, T?>> GetEnumerator() => _list.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

	public IEnumerable<UnityVersion> Keys => _list.Select(x => x.Key);

	public IEnumerable<T?> Values => _list.Select(x => x.Value);

	public override string ToString()
	{
		return $"{nameof(Count)} = {Count}";
	}
}
