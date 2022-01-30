using System;
using System.Collections.Generic;

namespace AssetRipper.Core.IO.DictionaryTest
{
	/// <summary>
	/// A wrapper for accessing the elements of an <see cref="NewAssetDictionary{TKey, TValue}"/>
	/// </summary>
	/// <typeparam name="SKey">The exposed key type</typeparam>
	/// <typeparam name="SValue">The exposed value type</typeparam>
	/// <typeparam name="TKey">The key type of the reference dictionary</typeparam>
	/// <typeparam name="TValue">The value type of the reference dictionary</typeparam>
	public class AccessDictionary<SKey, SValue, TKey, TValue> : IAccessDictionary<SKey, SValue> 
		where TKey : class, SKey, new()
		where TValue : class, SValue, new()
	{
		private readonly NewAssetDictionary<TKey, TValue> referenceDictionary;

		public AccessDictionary(NewAssetDictionary<TKey, TValue> referenceDictionary)
		{
			this.referenceDictionary = referenceDictionary;
		}

		public int Count => referenceDictionary.Count;

		public int Capacity => referenceDictionary.Capacity;

		public IEnumerable<SKey> Keys => referenceDictionary.Keys;

		public IEnumerable<SValue> Values => referenceDictionary.Values;

		public void Add(SKey key, SValue value)
		{
			if (key is not TKey compatibleKey)
			{
				throw new ArgumentException($"{key.GetType()} is not assignable to {typeof(TKey)}", nameof(newKey));
			}
			if (value is not TValue compatibleValue)
			{
				throw new ArgumentException($"{value.GetType()} is not assignable to {typeof(TValue)}", nameof(newValue));
			}
			referenceDictionary.Add(compatibleKey, compatibleValue);
		}

		public void Add(KeyValuePair<SKey, SValue> pair)
		{
			Add(pair.Key, pair.Value);
		}

		public void AddNew() => referenceDictionary.AddNew();

		public SKey GetKey(int index) => referenceDictionary.GetKey(index);

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

		public SValue GetValue(int index) => referenceDictionary.GetValue(index);

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
