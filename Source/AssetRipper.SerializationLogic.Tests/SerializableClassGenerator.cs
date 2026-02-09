using AsmResolver.DotNet.Cloning;

namespace AssetRipper.SerializationLogic.Tests;

internal static class SerializableClassGenerator
{
	public static TypeDefinition CreateEmptySerializableClass() => CreateEmptySerializableClass(null, null);
	public static TypeDefinition CreateEmptySerializableClass(string name) => CreateEmptySerializableClass(null, name);
	public static TypeDefinition CreateEmptySerializableClass(string? @namespace, string? name)
	{
		TypeDefinition emptySerializableClass = ReferenceAssemblies.GetType<EmptySerializableClass>();
		ModuleDefinition module = new("TestModule", ReferenceAssemblies.CorLib);
		MemberCloner cloner = new(module);
		cloner.Include(emptySerializableClass);
		TypeDefinition clonedType = cloner.Clone().ClonedTopLevelTypes.Single();
		if (!string.IsNullOrEmpty(@namespace))
		{
			clonedType.Namespace = @namespace;
		}
		if (!string.IsNullOrEmpty(name))
		{
			clonedType.Name = name;
		}
		module.TopLevelTypes.Add(clonedType);
		return clonedType;
	}
}
