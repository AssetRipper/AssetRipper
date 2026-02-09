namespace AssetRipper.AssemblyDumper.Types;

public static class StructCreator
{
	public const TypeAttributes SequentialStructAttributes =
		TypeAttributes.Public |
		TypeAttributes.SequentialLayout |
		TypeAttributes.Sealed |
		TypeAttributes.BeforeFieldInit;
	public const TypeAttributes ExplicitStructAttributes =
		TypeAttributes.Public |
		TypeAttributes.ExplicitLayout |
		TypeAttributes.Sealed |
		TypeAttributes.BeforeFieldInit;

	public static TypeDefinition CreateEmptyStruct(AssemblyBuilder builder, string @namespace, string name, bool sequential = true)
	{
		ITypeDefOrRef valueTypeReference = builder.Importer.ImportType<ValueType>();
		TypeAttributes typeAttributes = sequential ? SequentialStructAttributes : ExplicitStructAttributes;
		TypeDefinition definition = new TypeDefinition(@namespace, name, typeAttributes, valueTypeReference);
		builder.Module.TopLevelTypes.Add(definition);
		return definition;
	}

	public static TypeDefinition CreateEmptyStruct(AssemblyBuilder builder, TypeDefinition parentType, string name, bool sequential = true)
	{
		ITypeDefOrRef valueTypeReference = builder.Importer.ImportType<ValueType>();
		TypeAttributes typeAttributes = sequential ? SequentialStructAttributes : ExplicitStructAttributes;
		TypeDefinition definition = new TypeDefinition(parentType.Namespace, name, typeAttributes, valueTypeReference);
		parentType.NestedTypes.Add(definition);
		return definition;
	}
}
