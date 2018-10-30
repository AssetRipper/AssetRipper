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
				return IsFieldTypeSerializable(field.DeclaringType, field.FieldType, arguments);
			}
			return false;
		}

		public static bool IsFieldTypeSerializable(TypeReference declaringType, TypeReference fieldType, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			// if it's generic parameter then get its real type
			if (fieldType.IsGenericParameter)
			{
				GenericParameter parameter = (GenericParameter)fieldType;
				fieldType = arguments[parameter];
			}

			if (fieldType.IsArray)
			{
				ArrayType array = (ArrayType)fieldType;
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
				return IsFieldTypeSerializable(declaringType, elementType, arguments);
			}

			if (MonoType.IsList(fieldType))
			{
				// list is serialized same way as array, so check its argument
				GenericInstanceType list = (GenericInstanceType)fieldType;
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
				return IsFieldTypeSerializable(declaringType, listElement, arguments);
			}

			if (fieldType.IsPrimitive)
			{
				return true;
			}
			if (MonoType.IsString(fieldType))
			{
				return true;
			}
			if (MonoType.IsEngineStruct(fieldType))
			{
				return true;
			}
			if (MonoType.IsEnginePointer(fieldType))
			{
				return true;
			}

			if (IsRecursive(declaringType, fieldType))
			{
				return false;
			}
			if (fieldType.IsGenericInstance)
			{
				return false;
			}
			if (MonoType.IsObject(fieldType))
			{
				return false;
			}

			TypeDefinition definition = fieldType.Resolve();
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

		public static bool IsRecursive(TypeReference declaringType, TypeReference fieldType)
		{
			// "built in" primitive .NET types are placed into itself... it is so stupid
			// field.FieldType.IsPrimitive || MonoType.IsString(field.FieldType) || MonoType.IsEnginePointer(field.FieldType) => return false
			if (MonoType.IsDelegate(fieldType))
			{
				return false;
			}
			if (declaringType == fieldType)
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
