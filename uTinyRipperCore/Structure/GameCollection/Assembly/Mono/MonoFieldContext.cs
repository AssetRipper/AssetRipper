using Mono.Cecil;
using System.Collections.Generic;
using uTinyRipper.Layout;

namespace uTinyRipper.Game.Assembly.Mono
{
	public readonly struct MonoFieldContext
	{
		public MonoFieldContext(FieldDefinition field, AssetLayout layout) :
			this(field, null, layout)
		{
		}

		public MonoFieldContext(FieldDefinition field, IReadOnlyDictionary<GenericParameter, TypeReference> arguments, AssetLayout layout)
		{
			Layout = layout;
			Definition = field;
			ElementType = field.FieldType;
			IsArray = false;
			Arguments = arguments;
		}

		public MonoFieldContext(in MonoFieldContext copy, TypeReference fieldType, bool isArrayElement)
		{
			Layout = copy.Layout;
			Definition = copy.Definition;
			ElementType = fieldType;
			IsArray = isArrayElement;
			Arguments = copy.Arguments;
		}

		public AssetLayout Layout { get; }
		public FieldDefinition Definition { get; }
		public TypeReference DeclaringType => Definition.DeclaringType;
		public TypeReference ElementType { get; }
		public bool IsArray { get; }
		public IReadOnlyDictionary<GenericParameter, TypeReference> Arguments { get; }
	}
}
