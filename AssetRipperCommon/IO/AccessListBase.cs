using System.Collections;
using System.Collections.Generic;

namespace AssetRipper.Core.IO
{
	public abstract class AccessListBase<T> : IList<T>, IReadOnlyList<T>
	{
		/// <inheritdoc/>
		public abstract T this[int index] { get; set; }

		/// <inheritdoc/>
		public abstract int Count { get; }

		/// <summary>
		/// The capacity of the list 
		/// </summary>
		public abstract int Capacity { get; set; }

		/// <inheritdoc/>
		public bool IsReadOnly => false;

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

		/// <inheritdoc/>
		public abstract int IndexOf(T item);

		/// <inheritdoc/>
		public abstract void Insert(int index, T item);

		/// <inheritdoc/>
		public abstract bool Remove(T item);

		/// <inheritdoc/>
		public abstract void RemoveAt(int index);

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
	}
}
