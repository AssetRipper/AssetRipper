using AssetRipper.Core.Extensions;
using AssetRipper.Core.Structure.Assembly.Mono.Extensions;
using Mono.Cecil;
using System.Collections.Generic;
using System.Text;

namespace AssetRipper.Core.Structure.Assembly.Mono
{
	public readonly struct MonoTypeContext
	{
		public TypeReference Type { get; }
		/// <summary>
		/// Arguments of the declaring type, where current Type is located
		/// </summary>
		private IReadOnlyDictionary<GenericParameter, TypeReference> Arguments { get; }

		private static readonly Dictionary<GenericParameter, TypeReference> s_emptyArguments = new Dictionary<GenericParameter, TypeReference>(0);

		public MonoTypeContext(TypeReference type) : this(type, GetDeclaringArguments(type)) { }

		public MonoTypeContext(TypeReference type, MonoTypeContext context) : this(type, context.Arguments) { }

		public MonoTypeContext(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			Type = type;
			Arguments = arguments;
		}

		private static IReadOnlyDictionary<GenericParameter, TypeReference> GetDeclaringArguments(TypeReference type)
		{
			if (type.HasGenericParameters)
			{
				// if context get created for a template class, set arguments equals to itself
				Dictionary<GenericParameter, TypeReference> templateArguments = new Dictionary<GenericParameter, TypeReference>(type.GenericParameters.Count);
				foreach (GenericParameter parameter in type.GenericParameters)
				{
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
			IReadOnlyDictionary<GenericParameter, TypeReference> parentArguments = Arguments;
			if (parent.IsGenericInstance && parent.ContainsGenericParameter)
			{
				parentArguments = GetContextArguments();
			}
			return new MonoTypeContext(parent, parentArguments);
		}

		/// <summary>
		/// Replace all generic parameters with actual arguments
		/// </summary>
		/// <returns>Return new generic type if change was happened. Otherwise return a self copy</returns>
		public MonoTypeContext Resolve()
		{
			if (Type.ContainsGenericParameter)
			{
				return ResolveGenericParameter();
			}
			return this;
		}

		/// <summary>
		/// Appends method generic arguments to current ones
		/// </summary>
		/// <returns>Return context with merged arguments if change was happened. Otherwise return a self copy</returns>
		public MonoTypeContext Merge(MethodDefinition method)
		{
			if (method.HasGenericParameters)
			{
				int argsCount = method.GenericParameters.Count + Arguments.Count;
				Dictionary<GenericParameter, TypeReference> arguments = new Dictionary<GenericParameter, TypeReference>(argsCount);
				arguments.AddRange(Arguments);
				foreach (GenericParameter parameter in method.GenericParameters)
				{
					arguments.Add(parameter, parameter);
				}
				return new MonoTypeContext(Type, arguments);
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
			return Arguments;
		}

		public override string? ToString()
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
			switch (Type.GetEType())
			{
				case ElementType.Var:
				case ElementType.MVar:
					{
						GenericParameter parameter = (GenericParameter)Type;
						TypeReference resolvedType = Arguments[parameter];
						return new MonoTypeContext(resolvedType);
					}

				case ElementType.Array:
					{
						ArrayType array = (ArrayType)Type;
						MonoTypeContext arrayContext = new MonoTypeContext(array.ElementType, Arguments);
						MonoTypeContext resolvedContext = arrayContext.ResolveGenericParameter();
						ArrayType newArray = new ArrayType(resolvedContext.Type, array.Rank);
						if (array.Rank > 1)
						{
							for (int i = 0; i < array.Rank; i++)
							{
								newArray.Dimensions[i] = array.Dimensions[i];
							}
						}
						return new MonoTypeContext(newArray, Arguments);
					}

				case ElementType.GenericInst:
					{
						GenericInstanceType genericInstance = (GenericInstanceType)Type;
						GenericInstanceType newInstance = new GenericInstanceType(genericInstance.ElementType);
						foreach (TypeReference argument in genericInstance.GenericArguments)
						{
							MonoTypeContext argumentContext = new MonoTypeContext(argument, Arguments);
							MonoTypeContext resolvedArgument = argumentContext.Resolve();
							newInstance.GenericArguments.Add(resolvedArgument.Type);
						}
						return new MonoTypeContext(newInstance, Arguments);
					}

				case ElementType.ByRef:
					{
						ByReferenceType reference = (ByReferenceType)Type;
						MonoTypeContext refContext = new MonoTypeContext(reference.ElementType, Arguments);
						MonoTypeContext resolvedContext = refContext.ResolveGenericParameter();
						ByReferenceType newReference = new ByReferenceType(resolvedContext.Type);
						return new MonoTypeContext(newReference, Arguments);
					}

				case ElementType.Ptr:
					{
						PointerType pointer = (PointerType)Type;
						MonoTypeContext ptrContext = new MonoTypeContext(pointer.ElementType, Arguments);
						MonoTypeContext resolvedContext = ptrContext.ResolveGenericParameter();
						PointerType newPointer = new PointerType(resolvedContext.Type);
						return new MonoTypeContext(newPointer, Arguments);
					}

				case ElementType.Pinned:
					{
						PinnedType pinned = (PinnedType)Type;
						MonoTypeContext pinContext = new MonoTypeContext(pinned.ElementType, Arguments);
						MonoTypeContext resolvedContext = pinContext.ResolveGenericParameter();
						PinnedType newPinned = new PinnedType(resolvedContext.Type);
						return new MonoTypeContext(newPinned, Arguments);
					}

				case ElementType.FnPtr:
					{
						FunctionPointerType funcPtr = (FunctionPointerType)Type;
						FunctionPointerType newFuncPtr = new FunctionPointerType();
						newFuncPtr.HasThis = funcPtr.HasThis;
						newFuncPtr.ExplicitThis = funcPtr.ExplicitThis;
						newFuncPtr.CallingConvention = funcPtr.CallingConvention;
						MonoTypeContext returnContext = new MonoTypeContext(funcPtr.ReturnType, Arguments);
						MonoTypeContext resolvedReturn = returnContext.Resolve();
						newFuncPtr.ReturnType = resolvedReturn.Type;
						foreach (ParameterDefinition param in funcPtr.Parameters)
						{
							MonoTypeContext paramContext = new MonoTypeContext(param.ParameterType, Arguments);
							MonoTypeContext resolvedParam = paramContext.Resolve();
							ParameterDefinition newParameter = new ParameterDefinition(param.Name, param.Attributes, resolvedParam.Type);
							newFuncPtr.Parameters.Add(newParameter);
						}
						return new MonoTypeContext(newFuncPtr, Arguments);
					}

				default:
					throw new Exception($"Unknown generic parameter container {Type}");
			}
		}

	}
}
