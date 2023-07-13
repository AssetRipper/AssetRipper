namespace AssetRipper.Assets.Generics
{
	public sealed class AssetPair<TKey, TValue> : AccessPairBase<TKey, TValue>
		where TKey : notnull, new()
		where TValue : notnull, new()
	{
		private TKey key;
		private TValue value;

		public AssetPair()
		{
			key = new();
			value = new();
		}

		public override TKey Key
		{
			get => key;
			set
			{
				ThrowIfKeyNotImmutable();
				key = value;
			}
		}

		public override TValue Value
		{
			get => value;
			set
			{
				ThrowIfValueNotImmutable();
				this.value = value;
			}
		}

		private static void ThrowIfKeyNotImmutable()
		{
			if (!typeof(TKey).IsValueType && typeof(TKey) != typeof(string) && typeof(TKey) != typeof(Utf8String))
			{
				throw new NotSupportedException($"Only immutable values can be used in the setter for {nameof(Key)}.");
			}
		}

		private static void ThrowIfValueNotImmutable()
		{
			if (!typeof(TValue).IsValueType && typeof(TValue) != typeof(string) && typeof(TValue) != typeof(Utf8String))
			{
				throw new NotSupportedException($"Only immutable values can be used in the setter for {nameof(Value)}.");
			}
		}
	}
}
