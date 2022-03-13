using System;

namespace AssetRipper.Core.IO
{
	/// <summary>
	/// A wrapper for accessing the elements of an <see cref="AccessDictionaryBase{TKey, TValue}"/>
	/// </summary>
	/// <typeparam name="TKeyBase">The exposed key type, such as an interface</typeparam>
	/// <typeparam name="TValueBase">The exposed value type, such as an interface</typeparam>
	/// <typeparam name="TKey">The key type of the reference dictionary</typeparam>
	/// <typeparam name="TValue">The value type of the reference dictionary</typeparam>
	public sealed class AccessDictionary<TKey, TValue, TKeyBase, TValueBase> : AccessDictionaryBase<TKeyBase, TValueBase> 
		where TKey : TKeyBase //, new()
		where TValue : TValueBase //, new()
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
		public override void Add(NullableKeyValuePair<TKeyBase, TValueBase> pair) => Add(pair.Key, pair.Value);

		/// <inheritdoc/>
		public override void AddNew() => referenceDictionary.AddNew();

		/// <inheritdoc/>
		public override TKeyBase GetKey(int index) => referenceDictionary.GetKey(index);

		/// <inheritdoc/>
		public override void SetKey(int index, TKeyBase newKey) => referenceDictionary.SetKey(index, (TKey)newKey);

		/// <inheritdoc/>
		public override TValueBase GetValue(int index) => referenceDictionary.GetValue(index);

		/// <inheritdoc/>
		public override void SetValue(int index, TValueBase newValue) => referenceDictionary.SetValue(index, (TValue)newValue);

		/// <inheritdoc/>
		public override int IndexOf(NullableKeyValuePair<TKeyBase, TValueBase> item) => referenceDictionary.IndexOf(CastPair(item));

		/// <inheritdoc/>
		public override void Insert(int index, NullableKeyValuePair<TKeyBase, TValueBase> item) => referenceDictionary.Insert(index, CastPair(item));

		/// <inheritdoc/>
		public override void RemoveAt(int index) => referenceDictionary.RemoveAt(index);

		/// <inheritdoc/>
		public override void Clear() => referenceDictionary.Clear();

		/// <inheritdoc/>
		public override bool Contains(NullableKeyValuePair<TKeyBase, TValueBase> item) => referenceDictionary.Contains(CastPair(item));

		/// <inheritdoc/>
		public override void CopyTo(NullableKeyValuePair<TKeyBase, TValueBase>[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			if (arrayIndex < 0 || arrayIndex >= array.Length - Count)
				throw new ArgumentOutOfRangeException(nameof(arrayIndex));

			for(int i = 0; i < Count; i++)
			{
				array[i + arrayIndex] = this[i];
			}
		}

		/// <inheritdoc/>
		public override bool Remove(NullableKeyValuePair<TKeyBase, TValueBase> item) => referenceDictionary.Remove(CastPair(item));

		/// <inheritdoc/>
		public override NullableKeyValuePair<TKeyBase, TValueBase> this[int index]
		{
			get => CastPair(referenceDictionary[index]);
			set => referenceDictionary[index] = CastPair(value);
		}

		private static NullableKeyValuePair<TKey, TValue> CastPair(NullableKeyValuePair<TKeyBase, TValueBase> pair)
		{
			return new NullableKeyValuePair<TKey, TValue>((TKey)pair.Key, (TValue)pair.Value);
		}

		private static NullableKeyValuePair<TKeyBase, TValueBase> CastPair(NullableKeyValuePair<TKey, TValue> pair)
		{
			return new NullableKeyValuePair<TKeyBase, TValueBase>(pair.Key, pair.Value);
		}
	}
}
