﻿using AsmResolver.DotNet;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

internal static class AssemblyManagerExtensions
{
	public static IEnumerable<TypeDefinition> GetAllTypes(this IAssemblyManager manager)
	{
		return manager.GetAssemblies().SelectMany(a => a.Modules).SelectMany(m => m.GetAllTypes());
	}
}
