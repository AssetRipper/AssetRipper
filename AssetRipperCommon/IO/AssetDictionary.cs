﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using System.Collections.Generic;

namespace AssetRipper.Core.IO
{
	/// <summary>
	/// A dictionary class supporting non-unique keys
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
	public class AssetDictionary<TKey, TValue> : List<NullableKeyValuePair<TKey, TValue>>, IDependent
	{
		private static readonly bool isDependentType = NullableKeyValuePair<TKey, TValue>.IsDependentType;
		public void Add(TKey key, TValue value) => Add(new NullableKeyValuePair<TKey, TValue>(key, value));

		public AssetDictionary() { }
		public AssetDictionary(int capacity) : base(capacity) { }

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			if (isDependentType)
			{
				foreach (NullableKeyValuePair<TKey, TValue> keyValuePair in this)
				{
					if (keyValuePair != null)
					{
						foreach (PPtr<IUnityObjectBase> dependency in keyValuePair.FetchDependencies(context))
						{
							yield return dependency;
						}
					}
				}
			}
		}
	}

	public static class AssetDictionaryExtensions
	{
		public static NullableKeyValuePair<TKey, TConvert>[] ToCastedArray<TKey, TValue, TConvert>(this AssetDictionary<TKey, TValue> dictionary) where TValue : TConvert
		{
			var result = new NullableKeyValuePair<TKey, TConvert>[dictionary.Count];
			for (int i = 0; i < result.Length; i++)
			{
				var dictEntry = dictionary[i];
				result[i] = new NullableKeyValuePair<TKey, TConvert>(dictEntry.Key, dictEntry.Value);
			}
			return result;
		}

		public static NullableKeyValuePair<TKey, PPtr<TElement>>[] ToPPtrArray<TKey, TValue, TElement>(this AssetDictionary<TKey, TValue> dictionary) where TValue : IPPtr where TElement : IUnityObjectBase
		{
			var result = new NullableKeyValuePair<TKey, PPtr<TElement>>[dictionary.Count];
			for (int i = 0; i < result.Length; i++)
			{
				var dictEntry = dictionary[i];
				result[i] = new NullableKeyValuePair<TKey, PPtr<TElement>>(dictEntry.Key, new PPtr<TElement>(dictEntry.Value));
			}
			return result;
		}
	}
}
