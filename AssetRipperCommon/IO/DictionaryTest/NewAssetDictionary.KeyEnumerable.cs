using System.Collections;
using System.Collections.Generic;

namespace AssetRipper.Core.IO.DictionaryTest
{
	public partial class NewAssetDictionary<TKey, TValue> where TKey : new() 
		where TValue : new()
	{
		private class KeyEnumerable : IEnumerable<TKey>
		{
			private readonly NewAssetDictionary<TKey, TValue> dictionary;

			public KeyEnumerable(NewAssetDictionary<TKey, TValue> dictionary)
			{
				this.dictionary = dictionary;
			}

			public IEnumerator<TKey> GetEnumerator()
			{
				return new KeyEnumerator(dictionary);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}
