using Mono.Cecil;
using System;
using System.Collections.Generic;
using uTinyRipper.Converters.Script;
using uTinyRipper.Converters.Script.Mono;

namespace uTinyRipper.Game.Assembly.Mono
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
			if (IsSerializableArray(context.Type))
			{
				throw new ArgumentException(nameof(context));
			}

			manager.AddSerializableType(context.Type, this);
			Base = GetBaseType(manager, context);
			Fields = CreateFields(manager, context);
		}

#region Naming
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
#endregion
#region Helpers
		public static bool IsPrime(TypeReference type)
		{
			return IsPrime(type.Namespace, type.Name);
		}

		public static bool IsMonoPrime(TypeReference type)
		{
			return IsMonoPrime(type.Namespace, type.Name);
		}

		public static bool IsSerializableArray(TypeReference type)
		{
			return type.IsArray || IsList(type);
		}

		public static bool IsBuiltinGeneric(TypeReference type)
		{
			return IsList(type) || IsExposedReference(type);
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

		public static bool IsMonoDerived(TypeReference type)
		{
			while (type != null)
			{
				if (IsMonoPrime(type))
				{
					return true;
				}

				TypeDefinition definition = type.Resolve();
				type = definition.BaseType;
			}
			return false;
		}

		public static bool HasSerializeFieldAttribute(FieldDefinition field)
		{
			foreach (CustomAttribute attribute in field.CustomAttributes)
			{
				TypeReference type = attribute.AttributeType;
				if (IsSerializeFieldAttrribute(type.Namespace, type.Name))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsCompilerGenerated(TypeDefinition type)
		{
			foreach (CustomAttribute attr in type.CustomAttributes)
			{
				if (IsCompilerGeneratedAttrribute(attr.AttributeType.Namespace, attr.AttributeType.Name))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsCompilerGenerated(FieldDefinition field)
		{
			foreach (CustomAttribute attr in field.CustomAttributes)
			{
				if (IsCompilerGeneratedAttrribute(attr.AttributeType.Namespace, attr.AttributeType.Name))
				{
					return true;
				}
			}
			return false;
		}
#endregion
#region Serialization
		public static bool IsSerializable(in MonoFieldContext context)
		{
			if (IsSerializableModifier(context.Definition))
			{
				return IsFieldTypeSerializable(context);
			}
			return false;
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

		public static bool IsFieldTypeSerializable(in MonoFieldContext context)
		{
			TypeReference fieldType = context.ElementType;

			// if it's generic parameter then get its real type
			if (fieldType.IsGenericParameter)
			{
				GenericParameter parameter = (GenericParameter)fieldType;
				fieldType = context.Arguments[parameter];
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
					elementType = context.Arguments[parameter];
				}

				// array of arrays isn't serializable
				if (elementType.IsArray)
				{
					return false;
				}
				// array of serializable generics isn't serializable
				if (IsSerializableGeneric(elementType))
				{
					return false;
				}
				// check if array element is serializable
				MonoFieldContext elementScope = new MonoFieldContext(context, elementType, true);
				return IsFieldTypeSerializable(elementScope);
			}

			if (IsList(fieldType))
			{
				// list is serialized same way as array, so check its argument
				GenericInstanceType list = (GenericInstanceType)fieldType;
				TypeReference listElement = list.GenericArguments[0];

				// if it's generic parameter then get its real type
				if (listElement.IsGenericParameter)
				{
					GenericParameter parameter = (GenericParameter)listElement;
					listElement = context.Arguments[parameter];
				}

				// list of arrays isn't serializable
				if (listElement.IsArray)
				{
					return false;
				}
				// list of buildin generics isn't serializable
				if (IsBuiltinGeneric(listElement))
				{
					return false;
				}
				// check if list element is serializable
				MonoFieldContext elementScope = new MonoFieldContext(context, listElement, true);
				return IsFieldTypeSerializable(elementScope);
			}

			if (MonoUtils.IsSerializablePrimitive(fieldType))
			{
				return true;
			}
			if (IsObject(fieldType))
			{
				return false;
			}

			if (IsEngineStruct(fieldType))
			{
				return true;
			}
			if (fieldType.IsGenericInstance)
			{
				// even monobehaviour derived generic instances aren't serialiable
				return IsSerializableGeneric(fieldType);
			}
			if (IsMonoDerived(fieldType))
			{
				if (fieldType.ContainsGenericParameter)
				{
					return false;
				}
				return true;
			}

			if (IsRecursive(context.DeclaringType, fieldType))
			{
				return context.IsArray;
			}

			TypeDefinition definition = fieldType.Resolve();
			if (definition.IsInterface)
			{
				return false;
			}
			if (definition.IsAbstract)
			{
				return false;
			}
			if (IsCompilerGenerated(definition))
			{
				return false;
			}
			if (definition.IsEnum)
			{
				return true;
			}
			if (definition.IsSerializable)
			{
				if (ScriptExportManager.IsFrameworkLibrary(ScriptExportMonoType.GetModuleName(definition)))
				{
					return false;
				}
				if (definition.IsValueType && !context.Layout.IsStructSerializable)
				{
					return false;
				}
				return true;
			}

			return false;
		}

		private static bool IsRecursive(TypeReference declaringType, TypeReference fieldType)
		{
			// "built in" primitive .NET types are placed into itself... it is so stupid
			// field.FieldType.IsPrimitive || MonoType.IsString(field.FieldType) || MonoType.IsEnginePointer(field.FieldType) => return false
			if (IsDelegate(fieldType))
			{
				return false;
			}
			if (declaringType == fieldType)
			{
				return true;
			}
			return false;
		}
#endregion

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
			if (IsObject(resolvedContext.Type))
			{
				return null;
			}
			return manager.GetSerializableType(resolvedContext);
		}

		private static Field[] CreateFields(MonoManager manager, MonoTypeContext context)
		{
			List<Field> fields = new List<Field>();
			TypeDefinition definition = context.Type.Resolve();
			IReadOnlyDictionary<GenericParameter, TypeReference> arguments = context.GetContextArguments();
			foreach (FieldDefinition field in definition.Fields)
			{
				MonoFieldContext fieldContext = new MonoFieldContext(field, arguments, manager.Layout);
				if (IsSerializable(fieldContext))
				{
					MonoTypeContext typeContext = new MonoTypeContext(field.FieldType, arguments);
					MonoTypeContext resolvedContext = typeContext.Resolve();
					MonoTypeContext serFieldContext = GetSerializedElementContext(resolvedContext);
					SerializableType scriptType = manager.GetSerializableType(serFieldContext);
					bool isArray = IsSerializableArray(resolvedContext.Type);
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
