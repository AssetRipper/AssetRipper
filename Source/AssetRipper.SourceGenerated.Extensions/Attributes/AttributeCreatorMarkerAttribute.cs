namespace AssetRipper.SourceGenerated.Extensions.Attributes;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class AttributeCreatorMarkerAttribute(Type classIdType) : Attribute
{
	public Type ClassIdType { get; } = classIdType;
}
