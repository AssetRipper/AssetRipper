using Mono.Cecil;
using System.Collections.Generic;

namespace uTinyRipper.AssetExporters.Mono
{
	public sealed class MonoField : ScriptField
	{
		internal MonoField(FieldDefinition field, IReadOnlyDictionary<GenericParameter, TypeReference> arguments) :
			base(new MonoType(field.FieldType, arguments), IsArrayType(field.FieldType), field.Name)
		{
		}
		
		private MonoField(MonoField copy):
			base(copy)
		{
		}

		public static bool IsSerializableModifier(FieldDefinition field)
		{
			if (field.HasConstant)
			{
				return false;
			}
			if (field.IsStatic)
			{
				return false;
			}
			if (field.IsInitOnly)
			{
				return false;
			}
			if (IsCompilerGenerated(field))
			{
				return false;
			}
			if (IsRecursive(field))
			{
				return false;
			}

			if (field.IsPublic)
			{
				if (field.IsNotSerialized)
				{
					return false;
				}
				return true;
			}
			return HasSerializeFieldAttribute(field);
		}

		public static bool IsSerializable(FieldDefinition field, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			if(IsSerializableModifier(field))
			{
				return IsFieldTypeSerializable(field.FieldType, arguments);
			}
			return false;
		}

		public static bool IsFieldTypeSerializable(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			// if it's generic parameter then get its real type
			if (type.IsGenericParameter)
			{
				GenericParameter parameter = (GenericParameter)type;
				type = arguments[parameter];
			}

			if (type.IsArray)
			{
				ArrayType array = (ArrayType)type;
				// one dimention array only
				if (!array.IsVector)
				{
					return false;
				}

				// if it's generic parameter then get its real type
				TypeReference elementType = array.ElementType;
				if (elementType.IsGenericParameter)
				{
					GenericParameter parameter = (GenericParameter)elementType;
					elementType = arguments[parameter];
				}

				// array of arrays isn't serializable
				if (elementType.IsArray)
				{
					return false;
				}
				// array of lists isn't serializable
				if (MonoType.IsList(elementType))
				{
					return false;
				}
				// check if element is serializable
				return IsFieldTypeSerializable(elementType, arguments);
			}

			if (MonoType.IsList(type))
			{
				// list is serialized same way as array, so check its argument
				GenericInstanceType list = (GenericInstanceType)type;
				TypeReference listElement = list.GenericArguments[0];

				// if it's generic parameter then get its real type
				if (listElement.IsGenericParameter)
				{
					GenericParameter parameter = (GenericParameter)listElement;
					listElement = arguments[parameter];
				}

				// list of arrays isn't serializable
				if (listElement.IsArray)
				{
					return false;
				}
				// list of lists isn't serializable
				if (MonoType.IsList(listElement))
				{
					return false;
				}
				// check if element is serializable
				return IsFieldTypeSerializable(listElement, arguments);
			}

			if (type.IsPrimitive)
			{
				return true;
			}
			if (MonoType.IsString(type))
			{
				return true;
			}
			if (MonoType.IsEngineStruct(type))
			{
				return true;
			}
			if (MonoType.IsEnginePointer(type))
			{
				return true;
			}

			if (type.IsGenericInstance)
			{
				return false;
			}
			if (MonoType.IsObject(type))
			{
				return false;
			}

			TypeDefinition definition = type.Resolve();
			if (definition.IsInterface)
			{
				return false;
			}
			if(MonoType.IsCompilerGenerated(definition))
			{
				return false;
			}
			if (definition.IsSerializable)
			{
				return true;
			}
			if (definition.IsEnum)
			{
				return true;
			}

			return false;
		}

		public static bool IsRecursive(FieldDefinition field)
		{
			// "built in" primitive .NET types are placed into itself... it is so stupid
			if (field.FieldType.IsPrimitive)
			{
				return false;
			}
			if (MonoType.IsString(field.FieldType))
			{
				return false;
			}
			if (MonoType.IsDelegate(field.FieldType))
			{
				return false;
			}
			if (MonoType.IsEnginePointer(field.FieldType))
			{
				return false;
			}
			if (field.DeclaringType == field.FieldType)
			{
				return true;
			}
			return false;
		}

		public static bool IsCompilerGenerated(FieldDefinition field)
		{
			foreach (CustomAttribute attribute in field.CustomAttributes)
			{
				TypeReference type = attribute.AttributeType;
				if (IsCompilerGeneratedAttrribute(type.Namespace, type.Name))
				{
					return true;
				}
			}
			return false;
		}

		private static bool HasSerializeFieldAttribute(FieldDefinition field)
		{
			foreach (CustomAttribute attribute in field.CustomAttributes)
			{
				TypeReference type = attribute.AttributeType;
				if(IsSerializeFieldAttrribute(type.Namespace, type.Name))
				{
					return true;
				}
			}
			return false;
		}

		private static bool IsArrayType(TypeReference type)
		{
			return type.IsArray || MonoType.IsList(type);
		}

		public override IScriptField CreateCopy()
		{
			return new MonoField(this);
		}
	}
}
