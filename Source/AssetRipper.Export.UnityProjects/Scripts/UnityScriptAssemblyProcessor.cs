using AsmResolver.DotNet;
using AssetRipper.CIL;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Export.UnityProjects.Scripts;

/// <summary>
/// Makes the DOTS registry's data types accessible to decompiled game scripts.
/// </summary>
public static class UnityScriptAssemblyProcessor
{
	public static void Process(IAssemblyManager manager)
	{
		AssemblyDefinition? assembly = manager.GetAssemblies().FirstOrDefault(assembly => assembly.Name == "Unity.Entities");
		if (assembly?.ManifestModule is not ModuleDefinition module)
		{
			return;
		}
		MakeRegistryDataTypesAccessible(module);
		manager.ClearStreamCache();
	}

	public static void MakeRegistryDataTypesAccessible(ModuleDefinition module)
	{
		foreach ((string declaringName, string nestedName) in new[]
		{
			("TypeManager", "SystemTypeInfo"),
			("TypeRegistry", "SystemAttributeWithType"),
		})
		{
			if (module.TryGetTopLevelType("Unity.Entities", declaringName, out TypeDefinition? type))
			{
				TypeDefinition? nested = type.NestedTypes.FirstOrDefault(nested => nested.Name == nestedName);
				if (nested is not null)
				{
					nested.IsNestedPublic = true;
				}
			}
		}
	}
}
