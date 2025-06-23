namespace AssetRipper.SerializationLogic.Extensions;

public static class TypeDescriptorExtensions
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

	public static bool IsAssignableTo(this ITypeDescriptor typeRef, string ns, string name)
	{
		if (typeRef.ToTypeSignature() is GenericInstanceTypeSignature genericInstanceTypeSignature)
		{
			return genericInstanceTypeSignature.GenericType.IsAssignableTo(ns, name);
		}

		if (typeRef.Namespace == ns && typeRef.Name == name)
		{
			return true;
		}

		return typeRef.Resolve()?.IsSubclassOf(ns, name) ?? false;
		// If we can't resolve our typeref or one of its base types,
		// let's assume it is not assignable to our target type
	}

	public static bool IsAssignableTo(this ITypeDescriptor typeRef, string typeName)
	{
		if (typeRef.ToTypeSignature() is GenericInstanceTypeSignature genericInstanceTypeSignature)
		{
			return genericInstanceTypeSignature.GenericType.IsAssignableTo(typeName);
		}

		if (typeRef.FullName == typeName)
		{
			return true;
		}

		return typeRef.Resolve()?.IsSubclassOf(typeName) ?? false;
		// If we can't resolve our typeref or one of its base types,
		// let's assume it is not assignable to our target type
	}

	public static bool InheritsFromObject(this ITypeDescriptor type)
	{
		return type.IsAssignableTo("UnityEngine", "Object");
	}

	public static bool IsEnum(this ITypeDescriptor type)
	{
		return type.IsValueType && !type.IsPrimitive() && type.CheckedResolve().IsEnum;
	}

	public static bool IsStruct(this ITypeDescriptor type)
	{
		return type.IsValueType && !type.IsPrimitive() && !type.IsEnum() && !IsSystemDecimal(type);
	}

	public static bool IsPrimitive(this ITypeDescriptor type)
	{
		return type.ToTypeSignature() is CorLibTypeSignature;
	}

	public static bool IsArray(this ITypeDescriptor type)
	{
		if (type is TypeDefinition or TypeReference)
		{
			//Easy out without having to work out the type sig, which is slow
			return false;
		}

		return type.ToTypeSignature() is SzArrayTypeSignature or ArrayTypeSignature;
	}

	private static bool IsSystemDecimal(ITypeDescriptor type)
	{
		return type.FullName == "System.Decimal";
	}
}
