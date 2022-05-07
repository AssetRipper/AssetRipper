using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.IO
{
	public static class AssetDictionaryExtensions
	{
		public static NullableKeyValuePair<TKeyBase, TValueBase>[] ToCastedArray<TKey, TValue, TKeyBase, TValueBase>(this AssetDictionary<TKey, TValue> dictionary)
			where TKey : TKeyBase, new()
			where TValue : TValueBase, new()
		{
			var result = new NullableKeyValuePair<TKeyBase, TValueBase>[dictionary.Count];
			for (int i = 0; i < result.Length; i++)
			{
				var dictEntry = dictionary[i];
				result[i] = new NullableKeyValuePair<TKeyBase, TValueBase>(dictEntry.Key, dictEntry.Value);
			}
			return result;
		}

		public static NullableKeyValuePair<TKeyBase, PPtr<TValueElement>>[] ToPPtrArray<TKey, TValue, TKeyBase, TValueElement>(this AssetDictionary<TKey, TValue> dictionary) 
			where TKey : TKeyBase, new()
			where TValue : IPPtr, new()
			where TValueElement : IUnityObjectBase
		{
			var result = new NullableKeyValuePair<TKeyBase, PPtr<TValueElement>>[dictionary.Count];
			for (int i = 0; i < result.Length; i++)
			{
				var dictEntry = dictionary[i];
				result[i] = new NullableKeyValuePair<TKeyBase, PPtr<TValueElement>>(dictEntry.Key, new PPtr<TValueElement>(dictEntry.Value));
			}
			return result;
		}
	}
}
