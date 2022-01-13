using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using System.Collections.Generic;

namespace AssetRipper.Core.IO
{
	public class NullableKeyValuePair<TKey, TValue> : IDependent
	{
		static NullableKeyValuePair()
		{
			hasDependentKeys = typeof(IDependent).IsAssignableFrom(typeof(TKey));
			hasDependentValues = typeof(IDependent).IsAssignableFrom(typeof(TValue));
			IsDependentType = hasDependentKeys || hasDependentValues;
		}

		private static readonly bool hasDependentKeys;
		private static readonly bool hasDependentValues;
		public static bool IsDependentType { get; }
		public TKey Key { get; set; }
		public TValue Value { get; set; }

		public NullableKeyValuePair() { }

		public NullableKeyValuePair(TKey key, TValue value)
		{
			Key = key;
			Value = value;
		}

		public NullableKeyValuePair(KeyValuePair<TKey, TValue> pair)
		{
			Key = pair.Key;
			Value = pair.Value;
		}

		public static implicit operator KeyValuePair<TKey, TValue>(NullableKeyValuePair<TKey, TValue> nullable)
		{
			return nullable == null ? default : new KeyValuePair<TKey, TValue>(nullable.Key, nullable.Value);
		}

		public static implicit operator NullableKeyValuePair<TKey, TValue>(KeyValuePair<TKey, TValue> nonnullable)
		{
			return new NullableKeyValuePair<TKey, TValue>(nonnullable);
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			if (hasDependentKeys && Key != null && Key is IDependent keyDependent)
			{
				foreach (PPtr<IUnityObjectBase> dependency in keyDependent.FetchDependencies(context))
				{
					yield return dependency;
				}
			}
			if (hasDependentValues && Value != null && Value is IDependent valueDependent)
			{
				foreach (PPtr<IUnityObjectBase> dependency in valueDependent.FetchDependencies(context))
				{
					yield return dependency;
				}
			}
		}
	}
}
