using System.Collections;

namespace AssetRipper.SmartEnums;

internal readonly struct Sequence<T>(T[] values) : IEquatable<Sequence<T>>, IReadOnlyList<T>
{
	public T this[int index] => Values[index];

	public T[] Values { get; } = values;

	public int Count => Values.Length;

	public bool Equals(Sequence<T> other) => Values.SequenceEqual(other.Values);

	public override bool Equals(object obj) => obj is Sequence<T> other && Equals(other);

	public override int GetHashCode() => Values.Length; // Doesn't need to be good

	public static implicit operator Sequence<T>(T[] values) => new(values);

	public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Values).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => Values.GetEnumerator();
}
