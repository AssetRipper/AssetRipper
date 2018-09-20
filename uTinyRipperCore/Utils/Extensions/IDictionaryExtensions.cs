using System.Collections.Generic;

namespace uTinyRipper
{
	public static class IDictionaryExtensions
	{
		public static void AddRange<T1, T2>(this IDictionary<T1, T2> _this, IReadOnlyDictionary<T1, T2> source)
		{
			foreach (KeyValuePair<T1, T2> argument in source)
			{
				_this.Add(argument.Key, argument.Value);
			}
		}
	}
}
