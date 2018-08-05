using Mono.Cecil;

namespace UtinyRipper.AssetExporters.Mono
{
	public class MonoType : ScriptType
	{
		public MonoType(TypeReference type) :
			base(GetPrimitiveType(type), CreateComplexType(type))
		{
		}

		public static bool IsSerializableType(TypeReference type)
		{
			TypeReference elementType = GetElementType(type);
			if (elementType.IsPrimitive)
			{
				return true;
			}
			if (IsSerializableType(elementType.Namespace, elementType.Name))
			{
				return true;
			}
			if (IsEnginePointer(elementType))
			{
				return true;
			}

			TypeDefinition definition = elementType.Resolve();
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

		public static bool IsDelegate(TypeReference type)
		{
			return IsDelegate(type.Namespace, type.Name);
		}
		public static bool IsSystemObject(TypeReference type)
		{
			return IsObject(type.Namespace, type.Name);
		}
		public static bool IsString(TypeReference type)
		{
			return IsString(type.Namespace, type.Name);
		}
		public static bool IsList(TypeReference type)
		{
			return IsList(type.Namespace, type.Name);
		}


		public static bool IsUnityObject(TypeReference type)
		{
			return IsEngineObject(type.Namespace, type.Name);
		}
		public static bool IsScriptableObject(TypeReference type)
		{
			return IsScriptableObject(type.Namespace, type.Name);
		}
		public static bool IsComponent(TypeReference type)
		{
			return IsComponent(type.Namespace, type.Name);
		}
		public static bool IsBehaviour(TypeReference type)
		{
			return IsBehaviour(type.Namespace, type.Name);
		}
		public static bool IsMonoBehaviour(TypeReference type)
		{
			return IsMonoBehaviour(type.Namespace, type.Name);
		}
		public static bool IsEngineStruct(TypeReference type)
		{
			return IsEngineStruct(type.Namespace, type.Name);
		}

		public static bool IsPrime(TypeReference type)
		{
			return IsPrime(type.Namespace, type.Name);
		}		
		public static bool IsMonoPrime(TypeReference type)
		{
			return IsMonoPrime(type.Namespace, type.Name);
		}
		
		public static bool IsEnginePointer(TypeReference type)
		{
			if (IsSystemObject(type))
			{
				return false;
			}
			if (IsMonoPrime(type))
			{
				return true;
			}

			TypeDefinition definition = type.Resolve();
			return IsEnginePointer(definition.BaseType);
		}

		public static TypeReference GetElementType(TypeReference type)
		{
			if (type.IsArray)
			{
				return type.GetElementType();
			}
			if (IsList(type))
			{
				GenericInstanceType generic = (GenericInstanceType)type;
				return generic.GenericArguments[0];
			}
			return type;
		}

		private static PrimitiveType GetPrimitiveType(TypeReference type)
		{
			TypeReference elementType = GetElementType(type);
			TypeDefinition definition = elementType.Resolve();
			if(definition.IsEnum)
			{
				foreach(FieldDefinition field in definition.Fields)
				{
					if(field.Name == EnumValueFieldName)
					{
						elementType = field.FieldType;
						break;
					}
				}
			}
			return ToPrimitiveType(elementType);
		}

		private static IScriptStructure CreateComplexType(TypeReference type)
		{
			TypeReference elementType = GetElementType(type);
			if (IsEngineStruct(elementType))
			{
				return ScriptStructure.EngineTypeToScriptStructure(elementType.Name);
			}

			TypeDefinition definition = elementType.Resolve();
			if (definition.IsEnum)
			{
				return null;
			}

			PrimitiveType primType = ToPrimitiveType(elementType);
			if (primType == PrimitiveType.Complex)
			{
				if (IsEnginePointer(elementType))
				{
					return new ScriptPointer(elementType);
				}
				else
				{
					return new MonoStructure(definition);
				}
			}
			else
			{
				return null;
			}
		}

		private static PrimitiveType ToPrimitiveType(TypeReference type)
		{
			return ToPrimitiveType(type.Namespace, type.Name);
		}

		private const string EnumValueFieldName = "value__";
	}
}
