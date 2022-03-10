using System.Collections;
using System.Collections.Generic;

namespace AssetRipper.Core.IO
{
	/// <summary>
	/// Access the contents of a dictionary
	/// </summary>
	/// <typeparam name="TKey">The exposed key type, such as an interface, base type, or primitive type</typeparam>
	/// <typeparam name="TValue">The exposed value type, such as an interface, base type, or primitive type</typeparam>
	public abstract class AccessDictionaryBase<TKey, TValue> : IList<NullableKeyValuePair<TKey, TValue>>, IReadOnlyList<NullableKeyValuePair<TKey, TValue>>
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
		private KeyEnumerable keyEnumerable;

		/// <summary>
		/// The values in the dictionary
		/// </summary>
		public IEnumerable<TValue> Values => valueEnumerable ??= new ValueEnumerable(this);
		private ValueEnumerable valueEnumerable;

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
		/// Add a pair to the dictionary
		/// </summary>
		/// <remarks>
		/// This method is not necessarily type safe. 
		/// It could throw exceptions if used improperly.
		/// </remarks>
		/// <param name="pair">The pair to be added</param>
		public abstract void Add(NullableKeyValuePair<TKey, TValue> pair);

		/// <summary>
		/// Add a new pair to the dictionary
		/// </summary>
		public abstract void AddNew();

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

		/// <summary>
		/// Access a pair in the dictionary
		/// </summary>
		/// <remarks>
		/// The get method is type safe.
		/// The set method is not necessarily type safe
		/// and could throw exceptions if used improperly.
		/// </remarks>
		/// <param name="index">The index to access</param>
		/// <returns>The pair at that index</returns>
		public abstract NullableKeyValuePair<TKey, TValue> this[int index] { get; set; }

		/// <inheritdoc/>
		public bool IsReadOnly => false;

		/// <inheritdoc/>
		public abstract int IndexOf(NullableKeyValuePair<TKey, TValue> item);

		/// <inheritdoc/>
		public abstract void Insert(int index, NullableKeyValuePair<TKey, TValue> item);

		/// <inheritdoc/>
		public abstract void RemoveAt(int index);

		/// <inheritdoc/>
		public abstract void Clear();

		/// <inheritdoc/>
		public abstract bool Contains(NullableKeyValuePair<TKey, TValue> item);

		/// <inheritdoc/>
		public abstract void CopyTo(NullableKeyValuePair<TKey, TValue>[] array, int arrayIndex);

		/// <inheritdoc/>
		public abstract bool Remove(NullableKeyValuePair<TKey, TValue> item);

		/// <inheritdoc/>
		public IEnumerator<NullableKeyValuePair<TKey, TValue>> GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
			{
				yield return this[i];
			}
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
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

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}
