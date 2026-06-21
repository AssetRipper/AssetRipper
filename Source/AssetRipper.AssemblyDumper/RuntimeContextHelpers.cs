namespace AssetRipper.AssemblyDumper;

internal static class RuntimeContextHelpers
{
	public static GenericInstanceTypeSignature MakeGenericInstanceType(this ITypeDescriptor type, params IEnumerable<TypeSignature> typeArguments)
	{
		return TypeDescriptorExtensions.MakeGenericInstanceType(type, SharedState.Instance.RuntimeContext, typeArguments);
	}

	public static TypeSignature ToTypeSignature(this ITypeDescriptor type)
	{
		return type.ToTypeSignature(SharedState.Instance.RuntimeContext);
	}

	public static TypeSignature ToTypeSignature(this ITypeDefOrRef type)
	{
		return type.ToTypeSignature(SharedState.Instance.RuntimeContext);
	}

	public static TypeDefinition Resolve(this ITypeDescriptor type)
	{
		return type.Resolve(SharedState.Instance.RuntimeContext);
	}
}
