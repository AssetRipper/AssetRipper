using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;

namespace AssetRipper.SerializationLogic.Extensions
{
	public static class TypeDescriptorExtensions
	{
		public static string? SafeNamespace(this ITypeDescriptor type)
		{
			if (type.ToTypeSignature() is GenericInstanceTypeSignature genericInstanceTypeSignature)
			{
				return genericInstanceTypeSignature.GenericType.SafeNamespace();
			}

			if (type.DeclaringType is not null)
			{
				return type.DeclaringType.SafeNamespace();
			}

			return type.Namespace;
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
			return type.ToTypeSignature() is SzArrayTypeSignature or ArrayTypeSignature;
		}

		private static bool IsSystemDecimal(ITypeDescriptor type)
		{
			return type.FullName == "System.Decimal";
		}
	}
}
