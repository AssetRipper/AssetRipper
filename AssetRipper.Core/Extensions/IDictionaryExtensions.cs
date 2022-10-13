using System.Collections.Generic;

namespace AssetRipper.Core.Extensions
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

		public static T2 GetOrAdd<T1, T2>(this IDictionary<T1, T2> _this, T1 key) where T1 : notnull where T2 : new()
		{
			if (!_this.TryGetValue(key, out T2? value))
			{
				value = new();
				_this.Add(key, value);
			}
			return value;
		}

		public static T2 GetOrAdd<T1, T2>(this IDictionary<T1, T2> _this, T1 key, Func<T2> factory) where T1 : notnull
		{
			if (!_this.TryGetValue(key, out T2? value))
			{
				value = factory();
				_this.Add(key, value);
			}
			return value;
		}
	}
}
