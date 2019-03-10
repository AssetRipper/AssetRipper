using Mono.Cecil;
using System.Collections.Generic;

namespace uTinyRipper.AssetExporters.Mono
{
	public class MonoType : ScriptType
	{
		internal MonoType(MonoManager manager, TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments) :
			base(GetPrimitiveType(type, arguments))
		{
			string uniqueName = GetUniqueName(type);
			manager.AssemblyManager.AddScriptType(uniqueName, this);
			ComplexType = CreateComplexType(manager, type, arguments);
		}

		public static string GetUniqueName(TypeReference type)
		{
			return $"[{type.Module.Name}]{type.FullName}";
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
		public static bool IsExposedReference(TypeReference type)
		{
			return IsExposedReference(type.Namespace, type.Name);
		}

		public static bool IsPrime(TypeReference type)
		{
			return IsPrime(type.Namespace, type.Name);
		}
		public static bool IsMonoPrime(TypeReference type)
		{
			return IsMonoPrime(type.Namespace, type.Name);
		}

		public static bool IsBuiltinGeneric(TypeReference type)
		{
			return IsList(type) || IsExposedReference(type);
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

		public static bool IsSerializableGeneric(TypeReference type)
		{
			if (type.IsGenericInstance)
			{
				if (IsBuiltinGeneric(type))
				{
					return true;
				}

				TypeDefinition definition = type.Resolve();
				if (definition.IsEnum)
				{
					return true;
				}
			}
			return false;
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

		private static PrimitiveType GetPrimitiveType(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			TypeReference elementType = GetElementType(type, arguments);
			return ToPrimitiveType(elementType);
		}

		private static IScriptStructure CreateComplexType(MonoManager manager, TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			TypeReference elementType = GetElementType(type, arguments);
			if (IsEngineStruct(elementType))
			{
				return ScriptStructure.EngineTypeToScriptStructure(elementType.Name);
			}

			PrimitiveType primType = ToPrimitiveType(elementType);
			if (primType != PrimitiveType.Complex)
			{
				return null;
			}

			if (IsEnginePointer(elementType))
			{
				return new ScriptPointer(elementType);
			}
			else
			{
				return new MonoStructure(manager, elementType.Resolve(), arguments);
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
				return GetElementType(type, arguments);
			}
			if (IsList(type))
			{
				GenericInstanceType generic = (GenericInstanceType)type;
				type = generic.GenericArguments[0];
				return GetElementType(type, arguments);
			}

			return type;
		}

		public override IScriptStructure ComplexType { get; }

		private const string EnumValueFieldName = "value__";
	}
}
