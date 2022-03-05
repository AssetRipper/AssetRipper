namespace AssemblyDumper.Utils
{
	public static class InterfaceCreator
	{
		public static TypeDefinition CreateEmptyInterface(AssemblyDefinition assembly, string @namespace, string name)
		{
			ModuleDefinition? module = assembly.ManifestModule;
			TypeDefinition definition = new TypeDefinition(@namespace, name, TypeAttributes.Public | TypeAttributes.Interface);
			module!.TopLevelTypes.Add(definition);
			return definition;
		}

		public static TypeDefinition CreateEmptyInterface(AssemblyDefinition assembly, string @namespace, string name, ITypeDefOrRef[] interfaces)
		{
			TypeDefinition emptyInterface = CreateEmptyInterface(assembly, @namespace, name);
			foreach (ITypeDefOrRef implementedInterface in interfaces)
			{
				emptyInterface.Interfaces.Add(new InterfaceImplementation(implementedInterface));
			}

			return emptyInterface;
		}
	}
}