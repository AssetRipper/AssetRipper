using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace uTinyRipper.Assembly.Mono
{
	public struct MonoTypeContext
	{
		public MonoTypeContext(TypeReference type):
			this(type, GetDeclaringArguments(type))
		{
		}

		public MonoTypeContext(TypeReference type, MonoTypeContext context) :
			this(type, context.Arguments)
		{
		}

		public MonoTypeContext(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			Type = type;
			Arguments = arguments;
		}

		private static IReadOnlyDictionary<GenericParameter, TypeReference> GetDeclaringArguments(TypeReference type)
		{
			if (type.HasGenericParameters)
			{
				// if context get created with template class, set arguments equals to itself
				Dictionary<GenericParameter, TypeReference> templateArguments = new Dictionary<GenericParameter, TypeReference>(type.GenericParameters.Count);
				for (int i = 0; i < type.GenericParameters.Count; i++)
				{
					GenericParameter parameter = type.GenericParameters[i];
					templateArguments.Add(parameter, parameter);
				}
				return templateArguments;
			}
			return s_emptyArguments;
		}

		public MonoTypeContext GetBase()
		{
			// Resolve method returns template definition for GenericInstance
			TypeDefinition definition = Type.Resolve();
			TypeReference parent = definition.BaseType;
			IReadOnlyDictionary<GenericParameter, TypeReference> parentArguments = s_emptyArguments;
			if (parent.IsGenericInstance && parent.ContainsGenericParameter)
			{
				parentArguments = GetContextArguments();
			}
			return new MonoTypeContext(parent, parentArguments);
		}

		/// <summary>
		/// Replace all generic parameters with actual arguments
		/// </summary>
		/// <returns>Return new generic type if change was happened. Otherwise return itself</returns>
		public MonoTypeContext Resolve()
		{
			if (Type.ContainsGenericParameter)
			{
				return ResolveGenericParameter();
			}
			return this;
		}

		/// <summary>
		/// Get generic arguments of current Type
		/// </summary>
		public IReadOnlyDictionary<GenericParameter, TypeReference> GetContextArguments()
		{
			if (Type.IsGenericInstance)
			{
				GenericInstanceType instance = (GenericInstanceType)Type;
				// we need to get definition to get real generic parameters (T1, T2) instead of references ({!0}, {!1})
				TypeDefinition template = instance.ElementType.Resolve();
				Dictionary<GenericParameter, TypeReference> arguments = new Dictionary<GenericParameter, TypeReference>(template.GenericParameters.Count);
				for (int i = 0; i < template.GenericParameters.Count; i++)
				{
					GenericParameter parameter = template.GenericParameters[i];
					TypeReference argument = instance.GenericArguments[i];
					MonoTypeContext argumentContext = new MonoTypeContext(argument, Arguments);
					MonoTypeContext resolvedContext = argumentContext.Resolve();
					arguments.Add(parameter, resolvedContext.Type);
				}
				return arguments;
			}
			return s_emptyArguments;
		}

		public override string ToString()
		{
			if (Type == null)
			{
				return base.ToString();
			}
			if (Arguments.Count == 0)
			{
				return Type.FullName;
			}
			else
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(Type.FullName).Append('<');
				int i = 0;
				foreach (TypeReference argument in Arguments.Values)
				{
					sb.Append(argument.FullName);
					i++;
					if (i < Arguments.Count)
					{
						sb.Append(", ");
					}
				}
				sb.Append('>');
				return sb.ToString();
			}
		}

		/// <summary>
		/// Replace generic parameters with actual arguments
		/// </summary>
		private MonoTypeContext ResolveGenericParameter()
		{
			if (Type.IsGenericParameter)
			{
				GenericParameter parameter = (GenericParameter)Type;
				TypeReference resolvedType = Arguments[parameter];
				// TODO: check how T get replaced to T for Child<T> class
				return new MonoTypeContext(resolvedType);
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
				return new MonoTypeContext(array, s_emptyArguments);
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
					return new MonoTypeContext(genericInstance, s_emptyArguments);
				}
			}
			throw new Exception($"Unknown generic parameter container {Type}");
		}

		/// <summary>
		/// Replace generic parameters in generic instance with actual arguments
		/// </summary>
		private MonoTypeContext ResolveGenericInstanceParameters()
		{
			GenericInstanceType genericInstance = (GenericInstanceType)Type;
			GenericInstanceType newInstance = new GenericInstanceType(genericInstance.ElementType);
			foreach (TypeReference argument in genericInstance.GenericArguments)
			{
				MonoTypeContext argumentContext = new MonoTypeContext(argument, Arguments);
				MonoTypeContext resolvedContext = argumentContext.Resolve();
				newInstance.GenericArguments.Add(resolvedContext.Type);
			}
			return new MonoTypeContext(newInstance, s_emptyArguments);
		}

		public TypeReference Type { get; }
		/// <summary>
		/// Arguments of the declaring type, where current Type is located
		/// </summary>
		private IReadOnlyDictionary<GenericParameter, TypeReference> Arguments { get; }

		private static readonly Dictionary<GenericParameter, TypeReference> s_emptyArguments = new Dictionary<GenericParameter, TypeReference>(0);
	}
}
