using AssetRipper.Core.Layout;
using Mono.Cecil;
using System.Collections.Generic;

namespace AssetRipper.Core.Structure.Assembly.Mono
{
	public readonly struct MonoFieldContext
	{
		public MonoFieldContext(FieldDefinition field, LayoutInfo layout) : this(field, null, layout) { }

		public MonoFieldContext(FieldDefinition field, IReadOnlyDictionary<GenericParameter, TypeReference>? arguments, LayoutInfo layout)
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

		public LayoutInfo Layout { get; }
		public FieldDefinition Definition { get; }
		public TypeReference DeclaringType => Definition.DeclaringType;
		public TypeReference ElementType { get; }
		public bool IsArray { get; }
		public IReadOnlyDictionary<GenericParameter, TypeReference>? Arguments { get; }
	}
}
