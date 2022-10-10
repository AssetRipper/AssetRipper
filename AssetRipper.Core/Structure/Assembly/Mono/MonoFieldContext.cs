using Mono.Cecil;
using System.Collections.Generic;

namespace AssetRipper.Core.Structure.Assembly.Mono
{
	public readonly struct MonoFieldContext
	{
		public MonoFieldContext(FieldDefinition field, UnityVersion version) : this(field, null, version) { }

		public MonoFieldContext(FieldDefinition field, IReadOnlyDictionary<GenericParameter, TypeReference>? arguments, UnityVersion version)
		{
			IsStructSerializable = version.IsGreaterEqual(4, 5);
			Version = version;
			Definition = field;
			ElementType = field.FieldType;
			IsArray = false;
			Arguments = arguments;
		}

		public MonoFieldContext(in MonoFieldContext copy, TypeReference fieldType, bool isArrayElement)
		{
			IsStructSerializable = copy.IsStructSerializable;
			Version = copy.Version;
			Definition = copy.Definition;
			ElementType = fieldType;
			IsArray = isArrayElement;
			Arguments = copy.Arguments;
		}

		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public bool IsStructSerializable { get; }
		public UnityVersion Version { get; }
		public FieldDefinition Definition { get; }
		public TypeReference DeclaringType => Definition.DeclaringType;
		public TypeReference ElementType { get; }
		public bool IsArray { get; }
		public IReadOnlyDictionary<GenericParameter, TypeReference>? Arguments { get; }
	}
}
