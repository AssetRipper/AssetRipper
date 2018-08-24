using Mono.Cecil;
using System.Collections.Generic;

namespace UtinyRipper.AssetExporters.Mono
{
	public sealed class MonoStructure : ScriptStructure
	{
		internal MonoStructure(TypeDefinition type):
			this(type, s_emptyArguments)
		{
		}

		internal MonoStructure(TypeDefinition type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments) :
			base(type.Namespace, type.Name, CreateBase(type, arguments), CreateFields(type, arguments))
		{
		}

		private MonoStructure(MonoStructure copy) :
			base(copy)
		{
		}

		private static IScriptStructure CreateBase(TypeDefinition type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			if (MonoType.IsPrime(type.BaseType))
			{
				return null;
			}

			if(type.BaseType.IsGenericInstance)
			{
				Dictionary<GenericParameter, TypeReference> templateArguments = new Dictionary<GenericParameter, TypeReference>();
				GenericInstanceType instance = (GenericInstanceType)type.BaseType;
				TypeDefinition template = instance.ElementType.Resolve();
				for (int i = 0; i < instance.GenericArguments.Count; i++)
				{
					GenericParameter parameter = template.GenericParameters[i];
					TypeReference argument = instance.GenericArguments[i];
					if(argument.IsGenericParameter)
					{
						argument = arguments[(GenericParameter)argument];
					}
					templateArguments.Add(parameter, argument.Resolve());
				}

				return new MonoStructure(template, templateArguments);
			}

			TypeDefinition definition = type.BaseType.Resolve();
			return new MonoStructure(definition);
		}

		private static IEnumerable<IScriptField> CreateFields(TypeDefinition type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			List<IScriptField> fields = new List<IScriptField>();
			foreach (FieldDefinition field in type.Fields)
			{
				if (!MonoField.IsSerializable(field, arguments))
				{
					continue;
				}

				MonoField monoField = new MonoField(field, arguments);
				fields.Add(monoField);
			}
			return fields;
		}

		public override IScriptStructure CreateCopy()
		{
			return new MonoStructure(this);
		}

		private static readonly IReadOnlyDictionary<GenericParameter, TypeReference> s_emptyArguments = new Dictionary<GenericParameter, TypeReference>(0);
	}
}
