using Mono.Cecil;
using System.Collections.Generic;

namespace UtinyRipper.AssetExporters.Mono
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

			if (field.IsPublic)
			{
				if (field.IsNotSerialized)
				{
					return false;
				}
				return true;
			}
			else
			{
				foreach (CustomAttribute attr in field.CustomAttributes)
				{
					if (IsSerializeFieldAttrribute(attr))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool IsSerializable(FieldDefinition field, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			if(IsSerializableModifier(field))
			{
				return IsSerializable(field.FieldType, arguments);
			}
			return false;
		}

		private static bool IsSerializable(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
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

				// array of list isn't serializable
				if (MonoType.IsList(elementType))
				{
					return false;
				}
				// check if elelent is serializable
				return IsSerializable(elementType, arguments);
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

				// list of array isn't serializable
				if (listElement.IsArray)
				{
					return false;
				}
				// list of list isn't serializable
				if (MonoType.IsList(listElement))
				{
					return false;
				}
				// check if elelent is serializable
				return IsSerializable(listElement, arguments);
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
				// if generic instance contains argument other than generic parameter then it isn't serializable
				GenericInstanceType instance = (GenericInstanceType)type;
				foreach (TypeReference argument in instance.GenericArguments)
				{
					if (!argument.IsGenericParameter)
					{
						return false;
					}
				}
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

		private static bool IsSerializeFieldAttrribute(CustomAttribute attribute)
		{
			TypeReference type = attribute.AttributeType;
			return IsSerializeFieldAttrribute(type.Namespace, type.Name);
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
