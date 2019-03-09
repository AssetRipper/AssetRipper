using Mono.Cecil;
using System.Collections.Generic;

namespace uTinyRipper
{
	public static class MonoUtils
	{
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
	}
}
