namespace AssetRipper.SerializationLogic.Extensions;

internal static class ResolutionExtensions
{
	public static TypeDefinition? TryResolve(this ITypeDescriptor reference, RuntimeContext? runtimeContext)
	{
		return reference.TryResolve(runtimeContext, out TypeDefinition? definition) ? definition : null;
	}
}
