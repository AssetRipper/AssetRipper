namespace AssetRipper.AssemblyDumper.Types;

public static class StaticClassCreator
{
	public const TypeAttributes StaticClassAttributes =
		TypeAttributes.AnsiClass |
		TypeAttributes.BeforeFieldInit |
		TypeAttributes.Public |
		TypeAttributes.Sealed |
		TypeAttributes.Abstract;
	private const MethodAttributes StaticConstructorAttributes =
		MethodAttributes.Private |
		MethodAttributes.HideBySig |
		MethodAttributes.RuntimeSpecialName |
		MethodAttributes.SpecialName |
		MethodAttributes.Static;
	public const MethodAttributes StaticMethodAttributes =
		MethodAttributes.Public |
		MethodAttributes.HideBySig |
		MethodAttributes.Static;

	public static TypeDefinition CreateEmptyStaticClass(ModuleDefinition module, string? @namespace, string? name)
	{
		TypeDefinition newTypeDef = new TypeDefinition(@namespace, name, StaticClassAttributes);
		newTypeDef.BaseType = module.CorLibTypeFactory.Object.ToTypeDefOrRef();
		module.TopLevelTypes.Add(newTypeDef);

		return newTypeDef;
	}
}
