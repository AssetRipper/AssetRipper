using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace uTinyRipper.Assembly.Mono
{
	public class MonoType : SerializableType
	{
		internal MonoType(MonoManager manager, TypeReference type) :
			this(manager, new MonoTypeContext(type))
		{
		}

		internal MonoType(MonoManager manager, MonoTypeContext context) :
			base(context.Type.Namespace, ToPrimitiveType(context.Type), MonoUtils.GetName(context.Type))
		{
			if (context.Type.ContainsGenericParameter)
			{
				throw new ArgumentException(nameof(context));
			}
			if (MonoField.IsSerializableArray(context.Type))
			{
				throw new ArgumentException(nameof(context));
			}

			string uniqueName = GetUniqueName(context.Type);
			manager.AssemblyManager.AddSerializableType(uniqueName, this);
			Base = GetBaseType(manager, context);
			Fields = CreateFields(manager, context);
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

		private static SerializableType GetBaseType(MonoManager manager, MonoTypeContext context)
		{
			MonoTypeContext baseContext = context.GetBase();
			MonoTypeContext resolvedContext = baseContext.Resolve();
			TypeDefinition baseDefinition = resolvedContext.Type.Resolve();
			if (IsObject(baseDefinition))
			{
				return null;
			}
			return manager.GetSerializableType(resolvedContext);
		}

		private static Field[] CreateFields(MonoManager manager, MonoTypeContext context)
		{
			TypeDefinition definition = context.Type.Resolve();
			List<Field> fields = new List<Field>();
			IReadOnlyDictionary<GenericParameter, TypeReference> arguments = context.GetContextArguments();
			foreach (FieldDefinition field in definition.Fields)
			{
				if (MonoField.IsSerializable(field, arguments))
				{
					MonoTypeContext fieldContext = new MonoTypeContext(field.FieldType, arguments);
					MonoTypeContext resolvedContext = fieldContext.Resolve();
					MonoTypeContext serFieldContext = GetSerializedElementContext(resolvedContext);
					SerializableType scriptType = manager.GetSerializableType(serFieldContext);
					bool isArray = MonoField.IsSerializableArray(resolvedContext.Type);
					Field fieldStruc = new Field(scriptType, isArray, field.Name);
					fields.Add(fieldStruc);
				}
			}
			return fields.ToArray();
		}

		private static MonoTypeContext GetSerializedElementContext(MonoTypeContext context)
		{
			if (context.Type.IsArray)
			{
				ArrayType array = (ArrayType)context.Type;
				return new MonoTypeContext(array.ElementType);
			}
			if (IsList(context.Type))
			{
				GenericInstanceType generic = (GenericInstanceType)context.Type;
				return new MonoTypeContext(generic.GenericArguments[0]);
			}
			return context;
		}

		private const string EnumValueFieldName = "value__";
	}
}
