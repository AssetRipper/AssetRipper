namespace AssetRipper.AssemblyDumper.Types;

public static class InterfaceCreator
{
	public static TypeDefinition CreateEmptyInterface(ModuleDefinition module, string? @namespace, string? name)
	{
		TypeDefinition definition = new TypeDefinition(@namespace, name, TypeAttributes.Public | TypeAttributes.Interface);
		module!.TopLevelTypes.Add(definition);
		return definition;
	}

	public static TypeDefinition CreateEmptyInterface(ModuleDefinition module, string? @namespace, string? name, params ITypeDefOrRef[] interfaces)
	{
		TypeDefinition emptyInterface = CreateEmptyInterface(module, @namespace, name);
		foreach (ITypeDefOrRef implementedInterface in interfaces)
		{
			emptyInterface.AddInterfaceImplementation(implementedInterface);
		}

		return emptyInterface;
	}
}