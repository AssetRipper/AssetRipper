using System.Collections;

namespace AssetRipper.Assets.Generics;

/// <summary>
/// Access the contents of a dictionary
/// </summary>
/// <typeparam name="TKey">The exposed key type, such as an interface, base type, or primitive type</typeparam>
/// <typeparam name="TValue">The exposed value type, such as an interface, base type, or primitive type</typeparam>
public abstract class AccessDictionaryBase<TKey, TValue> : IReadOnlyCollection<AccessPairBase<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
	where TKey : notnull
	where TValue : notnull
{
	/// <summary>
	/// The capacity of the dictionary 
	/// </summary>
	public abstract int Capacity { get; set; }

	/// <summary>
	/// The number of pairs in the dictionary
	/// </summary>
	public abstract int Count { get; }

	/// <summary>
	/// The keys in the dictionary
	/// </summary>
	public IEnumerable<TKey> Keys => keyEnumerable ??= new KeyEnumerable(this);
	private KeyEnumerable? keyEnumerable;

	/// <summary>
	/// The values in the dictionary
	/// </summary>
	public IEnumerable<TValue> Values => valueEnumerable ??= new ValueEnumerable(this);
	private ValueEnumerable? valueEnumerable;

	/// <summary>
	/// Add a pair to the dictionary
	/// </summary>
	/// <remarks>
	/// This method is not necessarily type safe. 
	/// It could throw exceptions if used improperly.
	/// </remarks>
	/// <param name="key">The key to be added</param>
	/// <param name="value">The value to be added</param>
	public abstract void Add(TKey key, TValue value);

	/// <summary>
	/// Add a new pair to the dictionary
	/// </summary>
	public abstract AccessPairBase<TKey, TValue> AddNew();

	public bool ContainsKey(TKey key) => Keys.Contains(key);

	/// <summary>
	/// Get a key in the dictionary
	/// </summary>
	/// <param name="index">The index to access</param>
	/// <returns>The key at the specified index</returns>
	public abstract TKey GetKey(int index);

	/// <summary>
	/// Get a value in the dictionary
	/// </summary>
	/// <param name="index">The index to access</param>
	/// <returns>The value at the specified index</returns>
	public abstract TValue GetValue(int index);

	/// <summary>
	/// Get a pair in the dictionary
	/// </summary>
	/// <param name="index">The index to access</param>
	/// <returns>The pair at the specified index</returns>
	public abstract AccessPairBase<TKey, TValue> GetPair(int index);

	/// <summary>
	/// Set a key in the dictionary
	/// </summary>
	/// <remarks>
	/// This method is not necessarily type safe. 
	/// It could throw exceptions if used improperly.
	/// </remarks>
	/// <param name="index">The index to access</param>
	/// <param name="newKey">The new key to be assigned</param>
	public abstract void SetKey(int index, TKey newKey);

	/// <summary>
	/// Set a value in the dictionary
	/// </summary>
	/// <remarks>
	/// This method is not necessarily type safe. 
	/// It could throw exceptions if used improperly.
	/// </remarks>
	/// <param name="index">The index to access</param>
	/// <param name="newValue">The new value to be assigned</param>
	public abstract void SetValue(int index, TValue newValue);

	/// <inheritdoc/>
	public abstract void RemoveAt(int index);

	/// <inheritdoc/>
	public void RemoveAt(Index index)
	{
		RemoveAt(index.GetOffset(Count));
	}

	/// <inheritdoc/>
	public abstract void Clear();

	public AccessPairBase<TKey, TValue> GetSinglePairForKey(TKey key)
	{
		if (TryGetSinglePairForKey(key, out AccessPairBase<TKey, TValue>? pair))
		{
			return pair;
		}
		else
		{
			throw new KeyNotFoundException($"Key not found: {key}");
		}
	}

	public abstract bool TryGetSinglePairForKey(TKey key, [NotNullWhen(true)] out AccessPairBase<TKey, TValue>? pair);

	public abstract bool TryGetSinglePairForValue(TValue value, [NotNullWhen(true)] out AccessPairBase<TKey, TValue>? pair);

	/// <summary>
	/// Access a value in the dictionary
	/// </summary>
	/// <remarks>
	/// The get method is type safe.
	/// The set method is not necessarily type safe
	/// and could throw exceptions if used improperly.
	/// Both will throw if the key isn't unique.
	/// </remarks>
	public TValue this[TKey key]
	{
		get => GetSinglePairForKey(key).Value;
		set
		{
			if (TryGetSinglePairForKey(key, out AccessPairBase<TKey, TValue>? pair))
			{
				pair.Value = value;
			}
			else
			{
				Add(key, value);
			}
		}
	}

	public bool TryGetKey(TValue value, [NotNullWhen(true)] out TKey? key)
	{
		if (TryGetSinglePairForValue(value, out AccessPairBase<TKey, TValue>? pair))
		{
			key = pair.Key;
			return key is not null;
		}
		else
		{
			key = default;
			return false;
		}
	}

	public TKey? TryGetKey(TValue value)
	{
		TryGetKey(value, out TKey? key);
		return key;
	}

	public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
	{
		if (TryGetSinglePairForKey(key, out AccessPairBase<TKey, TValue>? pair))
		{
			value = pair.Value;
			return value is not null;
		}
		else
		{
			value = default;
			return false;
		}
	}

	public TValue? TryGetValue(TKey key)
	{
		TryGetValue(key, out TValue? value);
		return value;
	}

	/// <inheritdoc/>
	public abstract IEnumerator<AccessPairBase<TKey, TValue>> GetEnumerator();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public override string ToString()
	{
		return $"{nameof(Count)} = {Count}";
	}

	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		IEnumerator<AccessPairBase<TKey, TValue>> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			AccessPairBase<TKey, TValue> pair = enumerator.Current;
			yield return new KeyValuePair<TKey, TValue>(pair.Key, pair.Value);
		}
	}

	public bool TryAdd(TKey key, TValue value)
	{
		if (ContainsKey(key))
		{
			return false;
		}

		Add(key, value);
		return true;
	}

	private class KeyEnumerable : IEnumerable<TKey>
	{
		private readonly AccessDictionaryBase<TKey, TValue> dictionary;

		public KeyEnumerable(AccessDictionaryBase<TKey, TValue> dictionary)
		{
			this.dictionary = dictionary;
		}

		public IEnumerator<TKey> GetEnumerator()
		{
			for (int i = 0; i < dictionary.Count; i++)
			{
				yield return dictionary.GetKey(i);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private class ValueEnumerable : IEnumerable<TValue>
	{
		private readonly AccessDictionaryBase<TKey, TValue> dictionary;

		public ValueEnumerable(AccessDictionaryBase<TKey, TValue> dictionary)
		{
			this.dictionary = dictionary;
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			for (int i = 0; i < dictionary.Count; i++)
			{
				yield return dictionary.GetValue(i);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
