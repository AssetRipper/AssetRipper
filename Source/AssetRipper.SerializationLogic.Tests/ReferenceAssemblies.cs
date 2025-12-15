using System.Collections.Concurrent;
using System.Reflection;

namespace AssetRipper.SerializationLogic.Tests;

public static class ReferenceAssemblies
{
	public static AssemblyReference CorLib => (AssemblyReference)GetModule(typeof(ReferenceAssemblies)).CorLibTypeFactory.CorLibScope;

	private static ModuleDefinition GetModule<T>() => GetModule(typeof(T));
	private static ModuleDefinition GetModule(Type type) => GetModule(type.Assembly);
	private static ModuleDefinition GetModule(Assembly assembly)
	{
		return modules.GetOrAdd(assembly, a => new Lazy<ModuleDefinition>(() => ModuleDefinition.FromFile(a.Location), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
	}

	public static TypeDefinition GetType<T>() => GetType(typeof(T));
	public static TypeDefinition GetType(Type type)
	{
		ModuleDefinition module = GetModule(type);
		string fullName = type.FullName ?? throw new ArgumentException("Type has no FullName", nameof(type));

		Lazy<Dictionary<string, TypeDefinition>> lazyMap = typesCache.GetOrAdd(module, m => new Lazy<Dictionary<string, TypeDefinition>>(() =>
			m.GetAllTypes()
				.Where(t => t.FullName != null)
				.ToDictionary(t => t.FullName!, t => t, StringComparer.Ordinal), LazyThreadSafetyMode.ExecutionAndPublication));

		Dictionary<string, TypeDefinition> map = lazyMap.Value;

		if (!map.TryGetValue(fullName, out TypeDefinition? typeDef))
		{
			throw new InvalidOperationException($"Type '{fullName}' not found in module '{module.Name}'.");
		}

		return typeDef;
	}

	private static readonly ConcurrentDictionary<Assembly, Lazy<ModuleDefinition>> modules = new();
	private static readonly ConcurrentDictionary<ModuleDefinition, Lazy<Dictionary<string, TypeDefinition>>> typesCache = new();
}
