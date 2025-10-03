namespace AssetRipper.Assets.Generics;

/// <summary>
/// A wrapper for accessing the elements of an <see cref="AccessDictionaryBase{TKey, TValue}"/>
/// </summary>
/// <typeparam name="TKeyBase">The exposed key type, such as an interface</typeparam>
/// <typeparam name="TValueBase">The exposed value type, such as an interface</typeparam>
/// <typeparam name="TKey">The key type of the reference dictionary</typeparam>
/// <typeparam name="TValue">The value type of the reference dictionary</typeparam>
public sealed class AccessDictionary<TKey, TValue, TKeyBase, TValueBase> : AccessDictionaryBase<TKeyBase, TValueBase>
	where TKeyBase : notnull
	where TValueBase : notnull
	where TKey : notnull, TKeyBase, new()
	where TValue : notnull, TValueBase, new()
{
	private readonly AssetDictionary<TKey, TValue> referenceDictionary;

	public AccessDictionary(AssetDictionary<TKey, TValue> referenceDictionary)
	{
		this.referenceDictionary = referenceDictionary;
	}

	/// <inheritdoc/>
	public override int Count => referenceDictionary.Count;

	/// <inheritdoc/>
	public override int Capacity
	{
		get => referenceDictionary.Capacity;
		set => referenceDictionary.Capacity = value;
	}

	/// <inheritdoc/>
	public override void Add(TKeyBase key, TValueBase value) => referenceDictionary.Add((TKey)key, (TValue)value);

	/// <inheritdoc/>
	public override AccessPair<TKey, TValue, TKeyBase, TValueBase> AddNew() => new(referenceDictionary.AddNew());

	/// <inheritdoc/>
	public override TKeyBase GetKey(int index) => referenceDictionary.GetKey(index);

	/// <inheritdoc/>
	public override void SetKey(int index, TKeyBase newKey) => referenceDictionary.SetKey(index, (TKey)newKey);

	/// <inheritdoc/>
	public override TValueBase GetValue(int index) => referenceDictionary.GetValue(index);

	/// <inheritdoc/>
	public override void SetValue(int index, TValueBase newValue) => referenceDictionary.SetValue(index, (TValue)newValue);

	public override AccessPair<TKey, TValue, TKeyBase, TValueBase> GetPair(int index) => new(referenceDictionary.GetPair(index));

	/// <inheritdoc/>
	public override void RemoveAt(int index) => referenceDictionary.RemoveAt(index);

	/// <inheritdoc/>
	public override void Clear() => referenceDictionary.Clear();

	public override bool TryGetSinglePairForKey(TKeyBase key, [NotNullWhen(true)] out AccessPairBase<TKeyBase, TValueBase>? pair)
	{
		ArgumentNullException.ThrowIfNull(key);

		int hash = key.GetHashCode();
		bool found = false;
		pair = null;
		for (int i = Count - 1; i > -1; i--)
		{
			AccessPairBase<TKey, TValue> p = referenceDictionary.GetPair(i);
			if (p.Key.GetHashCode() == hash && key.Equals(p.Key))
			{
				if (found)
				{
					// Multiple entries found
					pair = null;
					return false;
				}
				else
				{
					found = true;
					pair = new AccessPair<TKey, TValue, TKeyBase, TValueBase>(p);
				}
			}
		}
		return found;
	}

	public override bool TryGetSinglePairForValue(TValueBase value, [NotNullWhen(true)] out AccessPairBase<TKeyBase, TValueBase>? pair)
	{
		ArgumentNullException.ThrowIfNull(value);

		int hash = value.GetHashCode();
		bool found = false;
		pair = null;
		for (int i = Count - 1; i > -1; i--)
		{
			AccessPairBase<TKey, TValue> p = referenceDictionary.GetPair(i);
			if (p.Value.GetHashCode() == hash && value.Equals(p.Value))
			{
				if (found)
				{
					// Multiple entries found
					pair = null;
					return false;
				}
				else
				{
					found = true;
					pair = new AccessPair<TKey, TValue, TKeyBase, TValueBase>(p);
				}
			}
		}
		return found;
	}

	/// <inheritdoc/>
	public override IEnumerator<AccessPair<TKey, TValue, TKeyBase, TValueBase>> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return GetPair(i);
		}
	}
}
