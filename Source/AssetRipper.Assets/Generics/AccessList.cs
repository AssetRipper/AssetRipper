namespace AssetRipper.Assets.Generics;

public sealed class AccessList<T, TBase> : AccessListBase<TBase>
	where TBase : notnull
	where T : notnull, TBase, new()
{
	private readonly AssetList<T> referenceList;

	public AccessList(AssetList<T> referenceList)
	{
		this.referenceList = referenceList;
	}

	/// <inheritdoc/>
	public override int Count => referenceList.Count;

	/// <inheritdoc/>
	public override int Capacity
	{
		get => referenceList.Capacity;
		set => referenceList.Capacity = value;
	}

	/// <inheritdoc/>
	public override void Add(TBase item) => referenceList.Add((T)item);

	/// <inheritdoc/>
	public override TBase AddNew() => referenceList.AddNew();

	/// <inheritdoc/>
	public override int IndexOf(TBase item) => referenceList.IndexOf((T)item);

	/// <inheritdoc/>
	public override void Insert(int index, TBase item) => referenceList.Insert(index, (T)item);

	/// <inheritdoc/>
	public override void RemoveAt(int index) => referenceList.RemoveAt(index);

	/// <inheritdoc/>
	public override void Clear() => referenceList.Clear();

	/// <inheritdoc/>
	public override bool Contains(TBase item) => referenceList.Contains((T)item);

	/// <inheritdoc/>
	public override void CopyTo(TBase[] array, int arrayIndex)
	{
		ArgumentNullException.ThrowIfNull(array);

		if (arrayIndex < 0 || arrayIndex > array.Length - Count)
		{
			throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, null);
		}

		for (int i = 0; i < Count; i++)
		{
			array[i + arrayIndex] = this[i];
		}
	}

	public override int EnsureCapacity(int capacity) => referenceList.EnsureCapacity(capacity);

	/// <inheritdoc/>
	public override bool Remove(TBase item) => referenceList.Remove((T)item);

	/// <inheritdoc/>
	public override TBase this[int index]
	{
		get => referenceList[index];
		set => referenceList[index] = (T)value;
	}
}
