namespace AssetRipper.Tests.Traversal;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class EditorFieldAttribute(string name) : Attribute
{
	public string Name { get; } = name;
}
