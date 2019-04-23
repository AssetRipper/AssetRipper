using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace uTinyRipper
{
	public static class MonoUtils
	{
		public static ArrayType CreateArrayFrom(ArrayType source, TypeReference newType)
		{
			ArrayType newArray = new ArrayType(newType, source.Rank);
			if (source.Rank > 1)
			{
				for (int i = 0; i < source.Rank; i++)
				{
					newArray.Dimensions[i] = source.Dimensions[i];
				}
			}
			return newArray;
		}

		public static GenericInstanceType CreateGenericInstance(TypeReference genericTemplate, IEnumerable<TypeReference> arguments)
		{
			GenericInstanceType genericInstance = new GenericInstanceType(genericTemplate);
			foreach (TypeReference argument in arguments)
			{
				genericInstance.GenericArguments.Add(argument);
			}
			return genericInstance;
		}

		public static int GetGenericArgumentCount(GenericInstanceType genericInstance)
		{
			int count = genericInstance.GenericArguments.Count;
			if (genericInstance.IsNested)
			{
				TypeReference declaring = genericInstance.DeclaringType;
				if (declaring.HasGenericParameters)
				{
					count -= declaring.GenericParameters.Count;
				}
			}
			return count;
		}

		public static int GetGenericParameterCount(TypeReference genericType)
		{
			int count = genericType.GenericParameters.Count;
			if (genericType.IsNested)
			{
				TypeReference declaring = genericType.DeclaringType;
				if (declaring.HasGenericParameters)
				{
					count -= declaring.GenericParameters.Count;
				}
			}
			return count;
		}

		/// <summary>
		/// Replace generic parameters with arguments
		/// </summary>
		public static TypeReference ResolveGenericParameter(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			if (type.IsGenericParameter)
			{
				GenericParameter generic = (GenericParameter)type;
				return arguments[generic];
			}
			if (type.IsArray)
			{
				ArrayType array = (ArrayType)type;
				TypeReference arrayElement = array.ElementType;
				TypeReference resolvedElement = arrayElement.ContainsGenericParameter ? ResolveGenericParameter(arrayElement, arguments) : arrayElement;
				return CreateArrayFrom(array, resolvedElement);
			}
			if (type.IsGenericInstance)
			{
				GenericInstanceType genericInstance = (GenericInstanceType)type;
				if (genericInstance.ContainsGenericParameter)
				{
					return ResolveGenericInstanceParameters(genericInstance, arguments);
				}
				return genericInstance;
			}
			throw new Exception($"Unknown generic parameter container {type}");
		}

		/// <summary>
		/// Replace generic parameters in generic instance with arguments
		/// </summary>
		public static GenericInstanceType ResolveGenericInstanceParameters(GenericInstanceType genericInstance, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			GenericInstanceType newInstance = new GenericInstanceType(genericInstance.ElementType);
			foreach (TypeReference argument in genericInstance.GenericArguments)
			{
				TypeReference newArgument = argument.ContainsGenericParameter ? ResolveGenericParameter(argument, arguments) : argument;
				newInstance.GenericArguments.Add(newArgument);
			}
			return newInstance;
		}
	}
}
