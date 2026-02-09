using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using System.Collections;
using System.Diagnostics;

namespace AssetRipper.Assets.Generics;

public abstract class AccessListBase<T> : IList<T>, IReadOnlyList<T>
	where T : notnull
{
	/// <inheritdoc/>
	public abstract T this[int index] { get; set; }

	/// <inheritdoc/>
	public abstract int Count { get; }

	/// <summary>
	/// The capacity of the list 
	/// </summary>
	public abstract int Capacity { get; set; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	bool ICollection<T>.IsReadOnly => false;

	/// <inheritdoc/>
	public abstract void Add(T item);

	/// <summary>
	/// Add a new element to the list
	/// </summary>
	public abstract T AddNew();

	/// <inheritdoc/>
	public abstract void Clear();

	/// <inheritdoc/>
	public abstract bool Contains(T item);

	/// <inheritdoc/>
	public abstract void CopyTo(T[] array, int arrayIndex);

	public abstract int EnsureCapacity(int capacity);

	/// <inheritdoc/>
	public abstract int IndexOf(T item);

	/// <inheritdoc/>
	public abstract void Insert(int index, T item);

	/// <inheritdoc/>
	public abstract bool Remove(T item);

	/// <inheritdoc/>
	public abstract void RemoveAt(int index);

	/// <inheritdoc cref="IList{T}.RemoveAt(int)"/>
	public void RemoveAt(Index index)
	{
		RemoveAt(index.GetOffset(Count));
	}

	/// <inheritdoc/>
	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return this[i];
		}
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public override string ToString()
	{
		return $"{nameof(Count)} = {Count}";
	}
}
public static class AccessListBaseExtensions
{
	public static PPtrAccessList<TPPtr, TAsset> ToPPtrAccessList<TPPtr, TAsset>(this AccessListBase<TPPtr> list, AssetCollection collection)
		where TPPtr : IPPtr<TAsset>
		where TAsset : IUnityObjectBase
	{
		return new PPtrAccessList<TPPtr, TAsset>(list, collection);
	}
}
