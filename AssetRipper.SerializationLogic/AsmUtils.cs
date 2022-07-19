using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AssetRipper.SerializationLogic.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.SerializationLogic
{
	public static class AsmUtils
	{
		public static IEnumerable<TypeDefinition> AllInterfacesImplementedBy(TypeDefinition typeDefinition)
		{
			return TypeAndBaseTypesOf(typeDefinition).SelectMany(t => t.Interfaces).Select(i => i.Interface!.CheckedResolve()).Distinct();
		}

		public static IEnumerable<TypeDefinition> TypeAndBaseTypesOf(ITypeDescriptor? typeReference)
		{
			while (typeReference != null)
			{
				TypeDefinition typeDefinition = typeReference.CheckedResolve();
				yield return typeDefinition;
				typeReference = typeDefinition.BaseType;
			}
		}

		public static IEnumerable<TypeDefinition> BaseTypesOf(ITypeDescriptor typeReference)
		{
			return TypeAndBaseTypesOf(typeReference).Skip(1);
		}

		public static bool IsGenericList(ITypeDescriptor type)
		{
			return type.Name == "List`1" && type.SafeNamespace() == "System.Collections.Generic";
		}

		public static bool IsGenericDictionary(ITypeDescriptor type)
		{
			if (type.ToTypeSignature() is GenericInstanceTypeSignature genericInstanceTypeSignature)
			{
				type = genericInstanceTypeSignature.GenericType;
			}

			return type.Name == "Dictionary`2" && type.SafeNamespace() == "System.Collections.Generic";
		}

		public static TypeSignature ElementTypeOfCollection(TypeSignature type)
		{
			if (type is SzArrayTypeSignature szArrayTypeSignature)
			{
				return szArrayTypeSignature.BaseType;
			}

			if (type is ArrayTypeSignature arrayTypeSignature)
			{
				return arrayTypeSignature.BaseType;
			}

			if (IsGenericList(type))
			{
				return ((GenericInstanceTypeSignature)type).TypeArguments.Single();
			}

			throw new ArgumentException("", nameof(type));
		}
	}
}
