using System;
using System.Collections.Generic;

namespace AssetRipper.Core.IO.DictionaryTest
{
	/// <summary>
	/// A wrapper for accessing the elements of an <see cref="NewAssetDictionary{TKey, TValue}"/>
	/// </summary>
	/// <typeparam name="SKey">The exposed key type, such as an interface</typeparam>
	/// <typeparam name="SValue">The exposed value type, such as an interface</typeparam>
	/// <typeparam name="TKey">The key type of the reference dictionary</typeparam>
	/// <typeparam name="TValue">The value type of the reference dictionary</typeparam>
	public class AccessDictionary<SKey, SValue, TKey, TValue> : IAccessDictionary<SKey, SValue> 
		where TKey : SKey, new()
		where TValue : SValue, new()
	{
		private readonly NewAssetDictionary<TKey, TValue> referenceDictionary;

		public AccessDictionary(NewAssetDictionary<TKey, TValue> referenceDictionary)
		{
			this.referenceDictionary = referenceDictionary;
		}

		/// <inheritdoc/>
		public int Count => referenceDictionary.Count;

		/// <inheritdoc/>
		public int Capacity => referenceDictionary.Capacity;

		/// <inheritdoc/>
		public IEnumerable<SKey> Keys
		{
			get
			{
				foreach(TKey key in referenceDictionary.Keys)
				{
					yield return key;
				}
			}
		}

		/// <inheritdoc/>
		public IEnumerable<SValue> Values
		{
			get
			{
				foreach(TValue value in referenceDictionary.Values)
				{
					yield return value;
				}
			}
		}

		/// <inheritdoc/>
		public void Add(SKey key, SValue value)
		{
			if (key is not TKey compatibleKey)
			{
				throw new ArgumentException($"{key.GetType()} is not assignable to {typeof(TKey)}", nameof(key));
			}
			if (value is not TValue compatibleValue)
			{
				throw new ArgumentException($"{value.GetType()} is not assignable to {typeof(TValue)}", nameof(value));
			}
			referenceDictionary.Add(compatibleKey, compatibleValue);
		}

		/// <inheritdoc/>
		public void Add(KeyValuePair<SKey, SValue> pair)
		{
			Add(pair.Key, pair.Value);
		}

		/// <inheritdoc/>
		public void AddNew() => referenceDictionary.AddNew();

		/// <inheritdoc/>
		public SKey GetKey(int index) => referenceDictionary.GetKey(index);

		/// <inheritdoc/>
		public void SetKey(int index, SKey newKey)
		{
			if (newKey is not TKey compatibleKey)
			{
				throw new ArgumentException($"{newKey.GetType()} is not assignable to {typeof(TKey)}", nameof(newKey));
			}
			else
			{
				referenceDictionary.SetKey(index, compatibleKey);
			}
		}

		/// <inheritdoc/>
		public SValue GetValue(int index) => referenceDictionary.GetValue(index);

		/// <inheritdoc/>
		public void SetValue(int index, SValue newValue)
		{
			if (newValue is not TValue compatibleValue)
			{
				throw new ArgumentException($"{newValue.GetType()} is not assignable to {typeof(TValue)}", nameof(newValue));
			}
			else
			{
				referenceDictionary.SetValue(index, compatibleValue);
			}
		}

		/// <inheritdoc/>
		public KeyValuePair<SKey, SValue> this[int index]
		{
			get => new KeyValuePair<SKey, SValue>(GetKey(index), GetValue(index));
			set
			{
				SetKey(index, value.Key);
				SetValue(index, value.Value);
			}
		}
	}
}
