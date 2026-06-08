namespace AssetRipper.SerializationLogic.Extensions;

internal static class TypeDescriptorExtensions
{
	public static string? SafeNamespace(this ITypeDescriptor type)
	{
		//We want to avoid calling ToTypeSignature where possible because it's expensive, so check each case manually.
		return type switch
		{
			TypeDefinition td => td.Namespace,
			TypeReference tr => tr.Namespace,
			TypeSpecification { Signature: GenericInstanceTypeSignature gts } => gts.GenericType.SafeNamespace(),
			TypeSpecification ts => ts.Namespace,
			GenericInstanceTypeSignature gts => gts.GenericType.SafeNamespace(),
			TypeSignature sig => sig.Namespace,
			_ => type.Namespace,
		};
	}

	public static bool IsAssignableTo(this ITypeDescriptor typeRef, string ns, string name, RuntimeContext? runtimeContext)
	{
		if (typeRef.ToTypeSignature(runtimeContext) is GenericInstanceTypeSignature genericInstanceTypeSignature)
		{
			return genericInstanceTypeSignature.GenericType.IsAssignableTo(ns, name, runtimeContext);
		}

		if (typeRef.Namespace == ns && typeRef.Name == name)
		{
			return true;
		}

		return typeRef.Resolve(runtimeContext)?.InheritsFrom(ns, name, runtimeContext) ?? false;
		// If we can't resolve our typeref or one of its base types,
		// let's assume it is not assignable to our target type
	}

	public static bool InheritsFromObject(this ITypeDescriptor type, RuntimeContext? runtimeContext)
	{
		return type.IsAssignableTo("UnityEngine", "Object", runtimeContext);
	}

	public static bool IsEnum(this ITypeDescriptor type, RuntimeContext? runtimeContext)
	{
		return type.GetIsValueType(runtimeContext) && !type.IsPrimitive(runtimeContext) && type.CheckedResolve(runtimeContext).IsEnum;
	}

	public static bool IsStruct(this ITypeDescriptor type, RuntimeContext? runtimeContext)
	{
		return type.GetIsValueType(runtimeContext) && !type.IsPrimitive(runtimeContext) && !type.IsEnum(runtimeContext) && !IsSystemDecimal(type);
	}

	public static bool IsPrimitive(this ITypeDescriptor type, RuntimeContext? runtimeContext)
	{
		return type.ToTypeSignature(runtimeContext) is CorLibTypeSignature;
	}

	public static bool IsArray(this ITypeDescriptor type, RuntimeContext? runtimeContext)
	{
		if (type is TypeDefinition or TypeReference)
		{
			//Easy out without having to work out the type sig, which is slow
			return false;
		}

		return type.ToTypeSignature(runtimeContext) is SzArrayTypeSignature or ArrayTypeSignature;
	}

	private static bool IsSystemDecimal(ITypeDescriptor type)
	{
		return type.FullName == "System.Decimal";
	}
}
