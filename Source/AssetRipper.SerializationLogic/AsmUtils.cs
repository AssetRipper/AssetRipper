using AssetRipper.SerializationLogic.Extensions;

namespace AssetRipper.SerializationLogic;

internal static class AsmUtils
{
	public static IEnumerable<TypeDefinition> AllInterfacesImplementedBy(TypeDefinition typeDefinition, RuntimeContext? runtimeContext)
	{
		return TypeAndBaseTypesOf(typeDefinition, runtimeContext).SelectMany(t => t.Interfaces).Select(i => i.Interface!.CheckedResolve(runtimeContext)).Distinct();
	}

	public static IEnumerable<TypeDefinition> TypeAndBaseTypesOf(ITypeDescriptor? typeReference, RuntimeContext? runtimeContext)
	{
		while (typeReference != null)
		{
			TypeDefinition typeDefinition = typeReference.CheckedResolve(runtimeContext);
			yield return typeDefinition;
			typeReference = typeDefinition.BaseType;
		}
	}

	public static IEnumerable<TypeDefinition> BaseTypesOf(ITypeDescriptor typeReference, RuntimeContext? runtimeContext)
	{
		return TypeAndBaseTypesOf(typeReference, runtimeContext).Skip(1);
	}

	public static bool IsGenericList(ITypeDescriptor type, RuntimeContext? runtimeContext)
	{
		return (type.ToTypeSignature(runtimeContext) as GenericInstanceTypeSignature)?.GenericType.Name == "List`1"
			&& type.SafeNamespace() == "System.Collections.Generic";
	}

	public static bool IsGenericDictionary(ITypeDescriptor type, RuntimeContext? runtimeContext)
	{
		if (type.ToTypeSignature(runtimeContext) is GenericInstanceTypeSignature genericInstanceTypeSignature)
		{
			type = genericInstanceTypeSignature.GenericType;
		}

		return type.Name == "Dictionary`2" && type.SafeNamespace() == "System.Collections.Generic";
	}

	public static TypeSignature ElementTypeOfCollection(TypeSignature type, RuntimeContext? runtimeContext)
	{
		if (type is SzArrayTypeSignature szArrayTypeSignature)
		{
			return szArrayTypeSignature.BaseType;
		}

		if (type is ArrayTypeSignature arrayTypeSignature)
		{
			return arrayTypeSignature.BaseType;
		}

		if (IsGenericList(type, runtimeContext))
		{
			return ((GenericInstanceTypeSignature)type).TypeArguments.Single();
		}

		throw new ArgumentException(null, nameof(type));
	}
}
