namespace AssetRipper.AssemblyDumper;

/// <summary>
/// <see href="https://github.com/Washi1337/AsmResolver/pull/752"/>
/// </summary>
internal static class GenericParamsCollectionPolyfill
{
	public static MethodSpecification MakeGenericInstanceMethod(this IMethodDefOrRef self, params IEnumerable<TypeSignature> arguments)
	{
		return MethodExtensions.MakeGenericInstanceMethod(self, arguments);
	}

	public static GenericInstanceTypeSignature MakeGenericInstanceType(this ITypeDescriptor type, RuntimeContext? context, params IEnumerable<TypeSignature> typeArguments)
	{
		return TypeDescriptorExtensions.MakeGenericInstanceType(type, context, typeArguments);
	}

	public static GenericInstanceTypeSignature MakeGenericInstanceType(this ITypeDescriptor type, bool isValueType, IEnumerable<TypeSignature> typeArguments)
	{
		return TypeDescriptorExtensions.MakeGenericInstanceType(type, isValueType, typeArguments);
	}

	public static GenericInstanceTypeSignature MakeGenericInstanceType(this ITypeDefOrRef type, bool isValueType, params IEnumerable<TypeSignature> typeArguments)
	{
		return TypeDescriptorExtensions.MakeGenericInstanceType(type, isValueType, typeArguments);
	}
}
