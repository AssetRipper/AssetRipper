using AsmResolver.DotNet;

namespace AssetRipper.Import.Structure.Assembly.Managers;

internal static class ModuleExtensions
{
	public static TypeDefinition? GetType(this ModuleDefinition module, string @namespace, string name)
	{
		IList<TypeDefinition> types = module.TopLevelTypes;
		foreach (TypeDefinition type in types)
		{
			if ((type.Namespace ?? AsmResolver.Utf8String.Empty) == @namespace && type.Name == name)
			{
				return type;
			}
		}

		return null;
	}
}
