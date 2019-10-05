using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace uTinyRipper.Assembly.Mono
{
	public struct MonoTypeContext
	{
		public MonoTypeContext(TypeReference type):
			this(type, s_emtryArguemnts)
		{
		}

		public MonoTypeContext(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			Type = type;
			Arguments = arguments;
		}

		/// <summary>
		/// Replace generic parameters with arguments
		/// </summary>
		public MonoTypeContext ResolveGenericParameter()
		{
			if (Type.IsGenericParameter)
			{
				GenericParameter generic = (GenericParameter)Type;
				TypeReference resolvedType = Arguments[generic];
				if (resolvedType.ContainsGenericParameter)
				{
					MonoTypeContext resolvedContext = new MonoTypeContext(resolvedType, Arguments);
					return resolvedContext.ResolveGenericParameter();
				}
				else
				{
					return new MonoTypeContext(resolvedType);
				}
			}
			if (Type.IsArray)
			{
				ArrayType array = (ArrayType)Type;
				TypeReference arrayElement = array.ElementType;
				if (arrayElement.ContainsGenericParameter)
				{
					MonoTypeContext arrayContext = new MonoTypeContext(arrayElement, Arguments);
					MonoTypeContext resolvedContext = arrayContext.ResolveGenericParameter();
					array = MonoUtils.CreateArrayFrom(array, resolvedContext.Type);
				}
				return new MonoTypeContext(array);
			}
			if (Type.IsGenericInstance)
			{
				GenericInstanceType genericInstance = (GenericInstanceType)Type;
				if (genericInstance.ContainsGenericParameter)
				{
					return ResolveGenericInstanceParameters();
				}
				else
				{
					return new MonoTypeContext(genericInstance);
				}
			}
			throw new Exception($"Unknown generic parameter container {Type}");
		}

		/// <summary>
		/// Replace generic parameters in generic instance with arguments
		/// </summary>
		public MonoTypeContext ResolveGenericInstanceParameters()
		{
			GenericInstanceType genericInstance = (GenericInstanceType)Type;
			GenericInstanceType newInstance = new GenericInstanceType(genericInstance.ElementType);
			Dictionary<GenericParameter, TypeReference> newArguments = new Dictionary<GenericParameter, TypeReference>(genericInstance.ElementType.GenericParameters.Count);
			for (int i = 0; i < genericInstance.GenericArguments.Count; i++)
			{
				GenericParameter parameter = genericInstance.ElementType.GenericParameters[i];
				TypeReference argument = genericInstance.GenericArguments[i];
				TypeReference newArgument;
				if (argument.ContainsGenericParameter)
				{
					MonoTypeContext argumentContext = new MonoTypeContext(argument, Arguments);
					MonoTypeContext newArgumentContext = argumentContext.ResolveGenericParameter();
					newArgument = newArgumentContext.Type;
				}
				else
				{
					newArgument = argument;
				}
				newInstance.GenericArguments.Add(newArgument);
				newArguments.Add(parameter, newArgument);
			}
			return new MonoTypeContext(newInstance, newArguments);
		}

		public TypeReference Type { get; }
		public IReadOnlyDictionary<GenericParameter, TypeReference> Arguments { get; }

		private static readonly Dictionary<GenericParameter, TypeReference> s_emtryArguemnts = new Dictionary<GenericParameter, TypeReference>(0);
	}
}
