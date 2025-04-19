using AsmResolver.DotNet;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

internal static class AssemblyManagerExtensions
{
	public static IEnumerable<ModuleDefinition> GetAllModules(this IAssemblyManager manager)
	{
		return manager.GetAssemblies().SelectMany(a => a.Modules);
	}

	public static IEnumerable<TypeDefinition> GetAllTypes(this IAssemblyManager manager)
	{
		return manager.GetAllModules().SelectMany(m => m.GetAllTypes());
	}

	public static IEnumerable<MethodDefinition> GetAllMethods(this IAssemblyManager manager)
	{
		return manager.GetAllTypes().SelectMany(t => t.Methods);
	}
}
