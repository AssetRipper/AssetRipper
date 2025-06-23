namespace AssetRipper.Assets.Generics;

public sealed class AccessPair<TKey, TValue, TKeyBase, TValueBase> : AccessPairBase<TKeyBase, TValueBase>
	where TKeyBase : notnull
	where TValueBase : notnull
	where TKey : notnull, TKeyBase, new()
	where TValue : notnull, TValueBase, new()
{
	private readonly AccessPairBase<TKey, TValue> referencePair;

	public AccessPair(AccessPairBase<TKey, TValue> pair)
	{
		referencePair = pair;
	}

	public override TKeyBase Key
	{
		get => referencePair.Key;
		set => referencePair.Key = (TKey)value;
	}

	public override TValueBase Value
	{
		get => referencePair.Value;
		set => referencePair.Value = (TValue)value;
	}
}
