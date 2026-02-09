namespace AssetRipper.AssemblyDumper;

internal class InjectedInterfaceProperty : InterfaceProperty
{
	public InjectedInterfaceProperty(PropertyDefinition definition, ClassGroupBase group) : base(definition, group)
	{
	}

	public override bool IsInjected => true;
}
