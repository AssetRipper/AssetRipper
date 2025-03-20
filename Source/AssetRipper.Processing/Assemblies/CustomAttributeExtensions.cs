using AsmResolver.DotNet;

namespace AssetRipper.Processing.Assemblies;

internal static class CustomAttributeExtensions
{
	public static bool IsCompilerGeneratedAttribute(this CustomAttribute attribute)
	{
		return attribute.Constructor?.DeclaringType is { Namespace.Value: "System.Runtime.CompilerServices", Name.Value: "CompilerGeneratedAttribute" };
	}
}
