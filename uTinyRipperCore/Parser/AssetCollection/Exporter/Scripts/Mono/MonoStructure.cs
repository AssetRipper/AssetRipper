using Mono.Cecil;
using System.Collections.Generic;

namespace uTinyRipper.AssetExporters.Mono
{
	public sealed class MonoStructure : ScriptStructure
	{
		internal MonoStructure(MonoManager manager, TypeDefinition type):
			this(manager, type, s_emptyArguments)
		{
		}

		internal MonoStructure(MonoManager manager, TypeDefinition type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments) :
			base(type.Namespace, type.Name, CreateBase(manager, type, arguments), CreateFields(manager, type, arguments))
		{
		}

		private MonoStructure(MonoStructure copy) :
			base(copy)
		{
		}

		private static IScriptStructure CreateBase(MonoManager manager, TypeDefinition type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
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

				return new MonoStructure(manager, template, templateArguments);
			}

			TypeDefinition definition = type.BaseType.Resolve();
			return new MonoStructure(manager, definition);
		}

		private static IEnumerable<IScriptField> CreateFields(MonoManager manager, TypeDefinition type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			List<IScriptField> fields = new List<IScriptField>();
			foreach (FieldDefinition field in type.Fields)
			{
				if (!MonoField.IsSerializable(field, arguments))
				{
					continue;
				}

				MonoField monoField = new MonoField(manager, field, arguments);
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
