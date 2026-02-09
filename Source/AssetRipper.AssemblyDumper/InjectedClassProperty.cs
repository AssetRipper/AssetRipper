namespace AssetRipper.AssemblyDumper;

internal class InjectedClassProperty : ClassProperty
{
	public InjectedClassProperty(PropertyDefinition definition, FieldDefinition? backingField, InterfaceProperty @base, GeneratedClassInstance @class) : base(definition, backingField, @base, @class)
	{
	}

	public override bool IsInjected => true;
}
