namespace AssetRipper.Tests.Traversal;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class ReleaseFieldAttribute(string name) : Attribute
{
	public string Name { get; } = name;
}
