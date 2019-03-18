using Mono.Cecil;
using System.Collections.Generic;

namespace uTinyRipper.Assembly.Mono
{
	public readonly struct MonoSerializableScope
	{
		public MonoSerializableScope(FieldDefinition field):
			this(field, null)
		{
		}

		public MonoSerializableScope(FieldDefinition field, IReadOnlyDictionary<GenericParameter, TypeReference> arguments) :
			this(field.DeclaringType, field.FieldType, false, arguments)
		{
		}

		public MonoSerializableScope(TypeReference declaringType, TypeReference fieldType, bool isArrayElement, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			DeclaringType = declaringType;
			FieldType = fieldType;
			IsArrayElement = isArrayElement;
			Arguments = arguments;
		}

		public TypeReference DeclaringType { get; }
		public TypeReference FieldType { get; }
		public bool IsArrayElement { get; }
		public IReadOnlyDictionary<GenericParameter, TypeReference> Arguments { get; }
	}
}
