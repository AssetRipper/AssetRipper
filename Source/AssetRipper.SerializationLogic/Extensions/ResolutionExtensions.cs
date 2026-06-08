namespace AssetRipper.SerializationLogic.Extensions;

internal static class ResolutionExtensions
{
	public static TypeDefinition CheckedResolve(this ITypeDescriptor reference, RuntimeContext? runtimeContext)
	{
		if (reference.ContextModule == null)
		{
			throw new ResolutionException(reference);
		}

		if (reference is not TypeDefinition definition)
		{
			definition = reference.Resolve(runtimeContext) ?? throw new ResolutionException(reference);
		}

		return definition;
	}

	public static MethodDefinition CheckedResolve(this IMethodDefOrRef reference, RuntimeContext? runtimeContext)
	{
		if (reference.ContextModule == null)
		{
			throw new ResolutionException(reference);
		}

		if (reference is not MethodDefinition definition)
		{
			definition = reference.Resolve(runtimeContext) ?? throw new ResolutionException(reference);
		}

		return definition;
	}
}
