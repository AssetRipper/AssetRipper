using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace uTinyRipper.Assembly.Mono
{
	public class MonoType : SerializableType
	{
		internal MonoType(MonoManager manager, TypeReference type) :
			this(manager, type, s_emptyArguments)
		{
		}

		internal MonoType(MonoManager manager, TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments) :
			base(type.Namespace, ToPrimitiveType(type), MonoUtils.GetName(type))
		{
			if (type.IsGenericParameter)
			{
				throw new ArgumentException(nameof(type));
			}
			if (MonoField.IsSerializableArray(type))
			{
				throw new ArgumentException(nameof(type));
			}

			string uniqueName = GetUniqueName(type);
			manager.AssemblyManager.AddSerializableType(uniqueName, this);
			Base = GetBaseType(manager, type, arguments);
			Fields = CreateFields(manager, type, arguments);
		}

		public static string GetUniqueName(TypeReference type)
		{
			string assembly = FilenameUtils.FixAssemblyEndian(type.Module.Name);
			return ScriptIdentifier.ToUniqueName(assembly, type.FullName);
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

		private static bool IsSerializableArray(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			TypeReference resolvedType = type.ContainsGenericParameter ? MonoUtils.ResolveGenericParameter(type, arguments) : type;
			return MonoField.IsSerializableArray(resolvedType);
		}

		private static SerializableType GetBaseType(MonoManager manager, TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			TypeDefinition definition = type.Resolve();
			if (IsObject(definition.BaseType))
			{
				return null;
			}

			if (definition.BaseType.IsGenericInstance)
			{
				Dictionary<GenericParameter, TypeReference> templateArguments = new Dictionary<GenericParameter, TypeReference>();
				GenericInstanceType instance = (GenericInstanceType)definition.BaseType;
				TypeDefinition template = instance.ElementType.Resolve();
				for (int i = 0; i < instance.GenericArguments.Count; i++)
				{
					GenericParameter parameter = template.GenericParameters[i];
					TypeReference argument = instance.GenericArguments[i];
					if (argument.ContainsGenericParameter)
					{
						argument = MonoUtils.ResolveGenericParameter(argument, arguments);
					}
					templateArguments.Add(parameter, argument);
				}
				return manager.GetSerializableType(instance, templateArguments);
			}
			else
			{
				TypeDefinition baseDefinition = definition.BaseType.Resolve();
				return manager.GetSerializableType(baseDefinition, s_emptyArguments);
			}
		}

		private static Field[] CreateFields(MonoManager manager, TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			TypeDefinition definition = type.Resolve();
			List<Field> fields = new List<Field>();
			foreach (FieldDefinition field in definition.Fields)
			{
				if (MonoField.IsSerializable(field, arguments))
				{
					TypeReference fieldType = GetSerializedElementType(field.FieldType, arguments);
					SerializableType scriptType = manager.GetSerializableType(fieldType, arguments);
					bool isArray = IsSerializableArray(field.FieldType, arguments);
					Field fieldStruc = new Field(scriptType, isArray, field.Name);
					fields.Add(fieldStruc);
				}
			}
			return fields.ToArray();
		}

		private static TypeReference GetSerializedElementType(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			TypeReference resolvedType = type.ContainsGenericParameter ? MonoUtils.ResolveGenericParameter(type, arguments) : type;
			if (resolvedType.IsArray)
			{
				ArrayType array = (ArrayType)resolvedType;
				return array.ElementType;
			}
			if (IsList(resolvedType))
			{
				GenericInstanceType generic = (GenericInstanceType)resolvedType;
				return generic.GenericArguments[0];
			}

			return resolvedType;
		}

		private const string EnumValueFieldName = "value__";

		private static readonly IReadOnlyDictionary<GenericParameter, TypeReference> s_emptyArguments = new Dictionary<GenericParameter, TypeReference>(0);
	}
}
