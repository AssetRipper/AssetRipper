using AsmResolver.DotNet;

namespace AssetRipper.DocExtraction.Extensions;

public static class TypeDefinitionExtensions
{
	public static bool IsStatic(this TypeDefinition type)
	{
		return type.IsAbstract && type.IsSealed;
	}
}