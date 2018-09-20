using Mono.Cecil;
using System.Collections.Generic;

namespace uTinyRipper.AssetExporters.Mono
{
	public class MonoType : ScriptType
	{
		internal MonoType(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments) :
			base(GetPrimitiveType(type, arguments), CreateComplexType(type, arguments))
		{
		}

		public static bool IsPrimitive(TypeReference type)
		{
			return IsPrimitive(type.Namespace, type.Name);
		}

		public static bool IsCPrimitive(TypeReference type)
		{
			return IsCPrimitive(type.Namespace, type.Name);
		}

		public static bool IsBasic(TypeReference type)
		{
			return IsBasic(type.Namespace, type.Name);
		}

		public static bool IsDelegate(TypeReference type)
		{
			return IsDelegate(type.Namespace, type.Name);
		}
		public static bool IsObject(TypeReference type)
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

		public static bool IsEngineObject(TypeReference type)
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
			if (IsObject(type))
			{
				return false;
			}
			if (IsMonoPrime(type))
			{
				return true;
			}

			TypeDefinition definition = type.Resolve();
			if (definition.IsInterface)
			{
				return false;
			}
			return IsEnginePointer(definition.BaseType);
		}

		public static bool IsCompilerGenerated(TypeDefinition type)
		{
			foreach (CustomAttribute attr in type.CustomAttributes)
			{
				if (attr.AttributeType.Name == CompilerGeneratedName && attr.AttributeType.Namespace == CompilerServicesNamespace)
				{
					return true;
				}
			}
			return false;
		}

		private static PrimitiveType GetPrimitiveType(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			type = GetElementType(type, arguments);
			return ToPrimitiveType(type);
		}

		private static IScriptStructure CreateComplexType(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			type = GetElementType(type, arguments);
			if (IsEngineStruct(type))
			{
				return ScriptStructure.EngineTypeToScriptStructure(type.Name);
			}

			PrimitiveType primType = ToPrimitiveType(type);
			if (primType == PrimitiveType.Complex)
			{
				if (IsEnginePointer(type))
				{
					return new ScriptPointer(type);
				}
				else
				{
					return new MonoStructure(type.Resolve(), arguments);
				}
			}
			else
			{
				return null;
			}
		}

		private static TypeReference GetElementType(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			if (type.IsGenericParameter)
			{
				GenericParameter parameter = (GenericParameter)type;
				type = arguments[parameter];
			}

			if (type.IsArray)
			{
				type = type.GetElementType();
			}
			else if (IsList(type))
			{
				GenericInstanceType generic = (GenericInstanceType)type;
				type = generic.GenericArguments[0];
			}

			if (type.IsGenericParameter)
			{
				GenericParameter parameter = (GenericParameter)type;
				type = arguments[parameter];
			}
			return type;
		}

		private static PrimitiveType ToPrimitiveType(TypeReference type)
		{
			TypeDefinition definition = type.Resolve();
			if (definition.IsEnum)
			{
				foreach (FieldDefinition field in definition.Fields)
				{
					if (field.Name == EnumValueFieldName)
					{
						type = field.FieldType;
						break;
					}
				}
			}

			return ToPrimitiveType(type.Namespace, type.Name);
		}

		private const string EnumValueFieldName = "value__";
	}
}
