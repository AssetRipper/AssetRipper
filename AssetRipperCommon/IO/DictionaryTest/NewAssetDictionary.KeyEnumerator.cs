using System.Collections;
using System.Collections.Generic;

namespace AssetRipper.Core.IO.DictionaryTest
{
	public partial class NewAssetDictionary<TKey, TValue> where TKey : new() 
		where TValue : new()
	{
		private class KeyEnumerator : IEnumerator<TKey>
		{
			private int currentIndex = -1;
			private readonly NewAssetDictionary<TKey, TValue> dictionary;

			public KeyEnumerator(NewAssetDictionary<TKey, TValue> dictionary)
			{
				this.dictionary = dictionary;
			}

			public TKey Current => dictionary.GetKey(currentIndex);

			object IEnumerator.Current => Current;

			public void Dispose() { }

			public bool MoveNext()
			{
				currentIndex++;
				return currentIndex < dictionary.Count;
			}

			public void Reset() => currentIndex = -1;
		}
	}
}
