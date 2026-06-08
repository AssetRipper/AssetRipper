using AsmResolver.DotNet.Cloning;

namespace AssetRipper.SerializationLogic.Tests;

internal static class SerializableClassGenerator
{
	public static TypeDefinition CreateEmptySerializableClass() => CreateEmptySerializableClass(null, null);
	public static TypeDefinition CreateEmptySerializableClass(string name) => CreateEmptySerializableClass(null, name);
	public static TypeDefinition CreateEmptySerializableClass(string? @namespace, string? name)
	{
		TypeDefinition emptySerializableClass = ReferenceAssemblies.GetType<EmptySerializableClass>();
		AssemblyDefinition assembly = new AssemblyDefinition("Test", new Version(1, 0, 0, 0));
		ModuleDefinition module = new("Test", ReferenceAssemblies.CorLib);
		assembly.Modules.Add(module);
		RuntimeContext runtimeContext = new(DotNetRuntimeInfo.NetCoreApp(ReferenceAssemblies.CorLib.Version), (bool?)null, ReferenceAssemblies.CorLib);
		runtimeContext.AddAssembly(assembly);
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
