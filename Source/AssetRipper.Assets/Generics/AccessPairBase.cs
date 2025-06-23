namespace AssetRipper.Assets.Generics;

public abstract class AccessPairBase<TKey, TValue> : IEquatable<AccessPairBase<TKey, TValue>>
	where TKey : notnull
	where TValue : notnull
{
	public abstract TKey Key { get; set; }
	public abstract TValue Value { get; set; }

	public sealed override bool Equals(object? obj)
	{
		return Equals(obj as AccessPairBase<TKey, TValue>);
	}

	public bool Equals(AccessPairBase<TKey, TValue>? other)
	{
		return other is not null &&
			EqualityComparer<TKey>.Default.Equals(Key, other.Key) &&
			EqualityComparer<TValue>.Default.Equals(Value, other.Value);
	}

	public sealed override int GetHashCode()
	{
		return HashCode.Combine(Key, Value);
	}

	public sealed override string ToString()
	{
		return $"{Key} : {Value}";
	}

	public static bool operator ==(AccessPairBase<TKey, TValue>? left, AccessPairBase<TKey, TValue>? right)
	{
		return EqualityComparer<AccessPairBase<TKey, TValue>>.Default.Equals(left, right);
	}

	public static bool operator !=(AccessPairBase<TKey, TValue>? left, AccessPairBase<TKey, TValue>? right)
	{
		return !(left == right);
	}

	public static implicit operator KeyValuePair<TKey, TValue>(AccessPairBase<TKey, TValue> pair) => new KeyValuePair<TKey, TValue>(pair.Key, pair.Value);

	public static implicit operator ValueTuple<TKey, TValue>(AccessPairBase<TKey, TValue> pair) => (pair.Key, pair.Value);

	public void Deconstruct(out TKey key, out TValue value)
	{
		key = Key;
		value = Value;
	}
}
