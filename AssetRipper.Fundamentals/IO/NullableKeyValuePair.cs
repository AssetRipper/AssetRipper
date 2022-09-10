using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using System.Collections.Generic;

namespace AssetRipper.Core.IO
{
	public class NullableKeyValuePair<TKey, TValue> : IDependent, IEquatable<NullableKeyValuePair<TKey, TValue>>
		where TKey : notnull
		where TValue : notnull
	{
		static NullableKeyValuePair()
		{
			hasDependentKeys = typeof(IDependent).IsAssignableFrom(typeof(TKey));
			hasDependentValues = typeof(IDependent).IsAssignableFrom(typeof(TValue));
			IsDependentType = hasDependentKeys || hasDependentValues;
		}

		private static readonly bool hasDependentKeys;
		private static readonly bool hasDependentValues;
		private TKey? key;
		private TValue? value;

		public static bool IsDependentType { get; }
		public TKey Key
		{
			get => key ?? throw new NullReferenceException(nameof(Key));
			set => key = value;
		}
		public TValue Value
		{
			get => value ?? throw new NullReferenceException(nameof(Value));
			set => this.value = value;
		}

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

		public static implicit operator KeyValuePair<TKey?, TValue?>(NullableKeyValuePair<TKey, TValue> nullable)
		{
			return nullable is null ? default : new KeyValuePair<TKey?, TValue?>(nullable.Key, nullable.Value);
		}

		public static implicit operator NullableKeyValuePair<TKey, TValue>(KeyValuePair<TKey, TValue> nonnullable)
		{
			return new NullableKeyValuePair<TKey, TValue>(nonnullable);
		}

		public static bool operator ==(NullableKeyValuePair<TKey, TValue>? left, NullableKeyValuePair<TKey, TValue>? right)
		{
			return EqualityComparer<NullableKeyValuePair<TKey, TValue>>.Default.Equals(left, right);
		}

		public static bool operator !=(NullableKeyValuePair<TKey, TValue>? left, NullableKeyValuePair<TKey, TValue>? right)
		{
			return !(left == right);
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

		public override bool Equals(object? obj)
		{
			return Equals(obj as NullableKeyValuePair<TKey, TValue>);
		}

		public bool Equals(NullableKeyValuePair<TKey, TValue>? other)
		{
			return other is not null &&
				   EqualityComparer<TKey>.Default.Equals(Key, other.Key) &&
				   EqualityComparer<TValue>.Default.Equals(Value, other.Value);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Key, Value);
		}

		public override string ToString()
		{
			return $"{key} : {value}";
		}
	}
}
