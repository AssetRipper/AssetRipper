namespace AssetRipper.Addressables;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
internal sealed class FormerlySerializedAsAttribute : Attribute
{
	public FormerlySerializedAsAttribute(string oldName)
	{
		OldName = oldName;
	}

	public string OldName { get; }
}
