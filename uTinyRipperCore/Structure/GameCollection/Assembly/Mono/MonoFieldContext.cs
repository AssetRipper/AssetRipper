using Mono.Cecil;
using System.Collections.Generic;

namespace uTinyRipper.Game.Assembly.Mono
{
	public readonly struct MonoFieldContext
	{
		public MonoFieldContext(FieldDefinition field, Version version) :
			this(field, null, version)
		{
		}

		public MonoFieldContext(FieldDefinition field, IReadOnlyDictionary<GenericParameter, TypeReference> arguments, Version version)
		{
			Version = version;
			Definition = field;
			ElementType = field.FieldType;
			IsArray = false;
			Arguments = arguments;
		}

		public MonoFieldContext(in MonoFieldContext copy, TypeReference fieldType, bool isArrayElement)
		{
			Version = copy.Version;
			Definition = copy.Definition;
			ElementType = fieldType;
			IsArray = isArrayElement;
			Arguments = copy.Arguments;
		}

		public Version Version { get; }
		public FieldDefinition Definition { get; }
		public TypeReference DeclaringType => Definition.DeclaringType;
		public TypeReference ElementType { get; }
		public bool IsArray { get; }
		public IReadOnlyDictionary<GenericParameter, TypeReference> Arguments { get; }
	}
}
