using System.Reflection;

namespace AssetRipper.SerializationLogic.Tests;

public static class ReferenceAssemblies
{
	public static ModuleDefinition GetModule<T>() => GetModule(typeof(T));
	public static ModuleDefinition GetModule(Type type) => GetModule(type.Assembly);
	public static ModuleDefinition GetModule(Assembly assembly)
	{
		if (!modules.TryGetValue(assembly, out ModuleDefinition? module))
		{
			module = ModuleDefinition.FromFile(assembly.Location);
			modules.Add(assembly, module);
		}
		return module;
	}

	public static TypeDefinition GetType<T>() => GetType(typeof(T));
	public static TypeDefinition GetType(Type type)
	{
		ModuleDefinition module = GetModule(type);
		string? fullName = type.FullName;
		return module.GetAllTypes().First(t => t.FullName == fullName);
	}

	private static readonly Dictionary<Assembly, ModuleDefinition> modules = new();
}
