using AsmResolver.DotNet;

namespace AssetRipper.Processing.Assemblies;

internal static class CustomAttributeExtensions
{
	public static bool IsType(this CustomAttribute attribute, string? @namespace, string? name)
	{
		ITypeDefOrRef? type = attribute.Constructor?.DeclaringType;
		return type is not null && type.Namespace == @namespace && type.Name == name;
	}

	public static bool IsCompilerGeneratedAttribute(this CustomAttribute attribute)
	{
		return attribute.IsType("System.Runtime.CompilerServices", "CompilerGeneratedAttribute");
	}
}
