using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;
using uTinyRipper.Game;
using uTinyRipper.Game.Assembly.Mono;

namespace uTinyRipper.Converters.Script.Mono
{
	public sealed class ScriptExportMonoType : ScriptExportType
	{
		public ScriptExportMonoType(TypeReference type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			Type = type;
			if (type.Module != null)
			{
				Definition = type.Resolve();
			}

			CleanName = GetSimpleName(Type);
			TypeName = GetTypeName(Type);
			NestedName = GetNestedName(Type, TypeName);
			CleanNestedName = ToCleanName(NestedName);
			Module = GetModuleName(Type);
			FullName = GetFullName(Type, Module);
		}

		public static string GetNestedName(TypeReference type)
		{
			string typeName = GetTypeName(type);
			return GetNestedName(type, typeName);
		}

		public static string GetNestedName(TypeReference type, string typeName)
		{
			if (type.IsGenericParameter)
			{
				return typeName;
			}
			if (type.IsArray)
			{
				return GetNestedName(type.GetElementType(), typeName);
			}
			if (type.IsNested)
			{
				string declaringName;
				if (type.IsGenericInstance)
				{
					GenericInstanceType generic = (GenericInstanceType)type;
					int argumentCount = MonoUtils.GetGenericArgumentCount(generic);
					List<TypeReference> genericArguments = new List<TypeReference>(generic.GenericArguments.Count - argumentCount);
					for (int i = 0; i < generic.GenericArguments.Count - argumentCount; i++)
					{
						genericArguments.Add(generic.GenericArguments[i]);
					}
					declaringName = GetNestedGenericName(type.DeclaringType, genericArguments);
				}
				else if (type.HasGenericParameters)
				{
					List<TypeReference> genericArguments = new List<TypeReference>(type.GenericParameters);
					declaringName = GetNestedGenericName(type.DeclaringType, genericArguments);
				}
				else
				{
					declaringName = GetNestedName(type.DeclaringType);
				}
				return $"{declaringName}.{typeName}";
			}
			return typeName;
		}

		public static string ToCleanName(string name)
		{
			int openIndex = name.IndexOf('<');
			if (openIndex == -1)
			{
				return name;
			}
			string firstPart = name.Substring(0, openIndex);
			int closeIndex = name.IndexOf('>');
			string secondPart = name.Substring(closeIndex + 1, name.Length - (closeIndex + 1));
			return firstPart + ToCleanName(secondPart);
		}

		public static string GetSimpleName(TypeReference type)
		{
			string name = type.Name;
			int index = name.IndexOf('`');
			if (index == -1)
			{
				return name;
			}

			bool strip = false;
			StringBuilder sb = new StringBuilder(name.Length);
			foreach (char c in name)
			{
				if (c == '`')
				{
					strip = true;
				}
				else if (!char.IsDigit(c))
				{
					strip = false;
				}

				if (!strip)
				{
					sb.Append(c);
				}
			}

			return sb.ToString();
		}

		public static string GetTypeName(TypeReference type)
		{
			if (MonoType.IsCPrimitive(type))
			{
				return MonoUtils.ToCPrimitiveString(type.Name);
			}

			if (type.IsGenericInstance)
			{
				GenericInstanceType generic = (GenericInstanceType)type;
				return GetGenericInstanceName(generic);
			}
			else if (type.HasGenericParameters)
			{
				return GetGenericTypeName(type);
			}
			else if (type.IsArray)
			{
				ArrayType array = (ArrayType)type;
				return GetTypeName(array.ElementType) + $"[{new string(',', array.Dimensions.Count - 1)}]";
			}
			return type.Name;
		}

		public static string GetFullName(TypeReference type)
		{
			string module = GetModuleName(type);
			return GetFullName(type, module);
		}

		public static string GetFullName(TypeReference type, string module)
		{
			string name = GetNestedName(type);
			string fullName = $"{type.Namespace}.{name}";
			return ScriptExportManager.ToFullName(module, fullName);
		}

		public static string GetModuleName(TypeReference type)
		{
			// reference and definition may has differrent module, so to avoid duplicates we need try to get defition
			TypeReference definition = type.ResolveOrDefault();
			definition = definition == null ? type : definition;
			return AssemblyManager.ToAssemblyName(definition.Scope.Name);
		}

		public static bool HasMember(TypeReference type, string name)
		{
			if (type == null)
			{
				return false;
			}
			if (type.Module == null)
			{
				return false;
			}
			TypeDefinition definition = type.Resolve();
			if (definition == null)
			{
				return false;
			}

			foreach (FieldDefinition field in definition.Fields)
			{
				if (field.Name == name)
				{
					return true;
				}
			}
			foreach (PropertyDefinition property in definition.Properties)
			{
				if (property.Name == name)
				{
					return true;
				}
			}
			return HasMember(definition.BaseType, name);
		}

		private static string GetNestedGenericName(TypeReference type, List<TypeReference> genericArguments)
		{
			string name = type.Name;
			if (type.HasGenericParameters)
			{
				name = GetGenericTypeName(type, genericArguments);
				int argumentCount = MonoUtils.GetGenericParameterCount(type);
				genericArguments.RemoveRange(genericArguments.Count - argumentCount, argumentCount);
			}
			if (type.IsNested)
			{
				string declaringName = GetNestedGenericName(type.DeclaringType, genericArguments);
				return $"{declaringName}.{name}";
			}
			else
			{
				return name;
			}
		}

		private static string GetGenericTypeName(TypeReference genericType)
		{
			// TypeReference contain parameters with "<!0,!1> (!index)" name but TypeDefinition's name is "<T1,T2> (RealParameterName)"
			genericType = genericType.ResolveOrDefault();
			return GetGenericName(genericType, genericType.GenericParameters);
		}

		private static string GetGenericTypeName(TypeReference genericType, IReadOnlyList<TypeReference> genericArguments)
		{
			genericType = genericType.ResolveOrDefault();
			return GetGenericName(genericType, genericArguments);
		}

		private static string GetGenericInstanceName(GenericInstanceType genericInstance)
		{
			return GetGenericName(genericInstance.ElementType, genericInstance.GenericArguments);
		}

		private static string GetGenericName(TypeReference genericType, IReadOnlyList<TypeReference> genericArguments)
		{
			string name = genericType.Name;
			int argumentCount = MonoUtils.GetGenericParameterCount(genericType);
			if (argumentCount == 0)
			{
				// nested class/enum (of generic class) is a generic instance but it doesn't have '`' symbol in its name
				return name;
			}

			int index = name.IndexOf('`');
			StringBuilder sb = new StringBuilder(genericType.Name, 0, index, 50 + index);
			sb.Append('<');
			for (int i = genericArguments.Count - argumentCount; i < genericArguments.Count; i++)
			{
				TypeReference arg = genericArguments[i];
				string argumentName = GetArgumentName(arg);
				sb.Append(argumentName);
				if (i < genericArguments.Count - 1)
				{
					sb.Append(", ");
				}
			}
			sb.Append('>');
			return sb.ToString();
		}

		private static string GetArgumentName(TypeReference type)
		{
			return GetNestedName(type);
		}

		public override void Init(IScriptExportManager manager)
		{
			if (Definition != null && Definition.BaseType != null)
			{
				m_base = manager.RetrieveType(Definition.BaseType);
			}

			m_constructor = CreateConstructor(manager);
			m_methods = CreateMethods(manager);
			m_properties = CreateProperties(manager);
			m_fields = CreateFields(manager);

			if (Type.IsNested)
			{
				m_declaringType = manager.RetrieveType(Type.DeclaringType);
				if (!Type.IsGenericParameter)
				{
					AddAsNestedType();
				}
			}
		}

		public override void GetUsedNamespaces(ICollection<string> namespaces)
		{
			if (Definition != null)
			{
				if (Definition.IsSerializable)
				{
					namespaces.Add(ScriptExportAttribute.SystemNamespace);
				}
			}

			base.GetUsedNamespaces(namespaces);
		}

		public override bool HasMember(string name)
		{
			if (base.HasMember(name))
			{
				return true;
			}
			return HasMember(Type, name);
		}

		private ScriptExportConstructor CreateConstructor(IScriptExportManager manager)
		{
			if (Definition == null)
			{
				return null;
			}

			// find the constructors to generate by simply taking the first constructor with the least arguments that has maximum accessibility
			MethodDefinition ctor = null;
			MethodAttributes ctorAttributes = 0;
			foreach (MethodDefinition method in Definition.Methods)
			{
				if (method.IsConstructor && !method.IsStatic)
				{
					MethodAttributes attributes;
					if (method.IsPublic)
					{
						attributes = MethodAttributes.Public;
					}
					else if (method.IsFamilyOrAssembly || method.IsFamily)
					{
						attributes = MethodAttributes.Family;
					}
					else if (method.IsAssembly)
					{
						attributes = MethodAttributes.Assembly;
					}
					else
					{
						attributes = MethodAttributes.Private;
					}

					if (ctor == null || attributes > ctorAttributes || (attributes == ctorAttributes && ctor.Parameters.Count > method.Parameters.Count))
					{
						ctor = method;
						ctorAttributes = attributes;
					}
				}
			}
			if (ctor != null)
			{
				ScriptExportConstructor constructor = manager.RetrieveConstructor(ctor);
				// if public, protected or internal constructor doesn't exist we need to generate a private constructor
				// to avoid the compiler generating a parameterless public one that didn't exist before
				if (ctorAttributes == MethodAttributes.Private)
				{
					return constructor;
				}
				if (constructor.Parameters.Count != 0)
				{
					return constructor;
				}
				if ((constructor.Base?.Parameters.Count ?? 0) != 0)
				{
					return constructor;
				}
			}
			return null;
		}

		private IReadOnlyList<ScriptExportMethod> CreateMethods(IScriptExportManager manager)
		{
			if (Definition == null || Definition.BaseType == null)
			{
				return Array.Empty<ScriptExportMethod>();
			}

			// we need to export only such methods that are declared as abstract inside builtin assemblies
			// and not overridden anywhere except current type
			List<ScriptExportMethod> methods = new List<ScriptExportMethod>();
			List<MethodDefinition> overrides = new List<MethodDefinition>();
			foreach (MethodDefinition method in Definition.Methods)
			{
				if (method.IsVirtual && method.IsReuseSlot && !method.IsGetter && !method.IsSetter)
				{
					overrides.Add(method);
				}
			}

			TypeDefinition definition = Definition;
			MonoTypeContext context = new MonoTypeContext(Definition);
			while (true)
			{
				if (overrides.Count == 0)
				{
					break;
				}
				if (definition.BaseType == null || definition.BaseType.Module == null)
				{
					break;
				}

				context = context.GetBase();
				definition = context.Type.Resolve();
				if (definition == null)
				{
					break;
				}

				string module = GetModuleName(definition);
				bool isBuiltIn = ScriptExportManager.IsBuiltinLibrary(module);
				IReadOnlyDictionary<GenericParameter, TypeReference> arguments = context.GetContextArguments();
				// definition is a Template for GenericInstance, so we must recreate context
				MonoTypeContext definitionContext = new MonoTypeContext(definition, arguments);
				foreach (MethodDefinition method in definition.Methods)
				{
					if (method.IsVirtual && (method.IsNewSlot || method.IsReuseSlot))
					{
						for (int i = 0; i < overrides.Count; i++)
						{
							MethodDefinition @override = overrides[i];
							if (MonoUtils.AreSame(@override, definitionContext, method))
							{
								if (isBuiltIn && method.IsAbstract)
								{
									ScriptExportMethod exportMethod = manager.RetrieveMethod(@override);
									methods.Add(exportMethod);
								}

								overrides.RemoveAt(i);
								break;
							}
						}
					}
				}
			}
			return methods.ToArray();
		}

		private IReadOnlyList<ScriptExportProperty> CreateProperties(IScriptExportManager manager)
		{
			if (Definition == null || Definition.BaseType == null || Definition.BaseType.Module == null)
			{
				return Array.Empty<ScriptExportProperty>();
			}

			// we need to export only such properties that are declared as asbtract inside builin assemblies
			// and not overridden anywhere except current type
			List<PropertyDefinition> overrides = new List<PropertyDefinition>();
			foreach (PropertyDefinition property in Definition.Properties)
			{
				MethodDefinition method = property.GetMethod == null ? property.SetMethod : property.GetMethod;
				if (method.IsVirtual && method.IsReuseSlot)
				{
					overrides.Add(property);
				}
			}

			List<ScriptExportProperty> properties = new List<ScriptExportProperty>();
			MonoTypeContext context = new MonoTypeContext(Definition);
			TypeDefinition definition = Definition;
			while (true)
			{
				if (overrides.Count == 0)
				{
					break;
				}
				if (definition.BaseType == null || definition.BaseType.Module == null)
				{
					break;
				}

				context = context.GetBase();
				definition = context.Type.Resolve();
				if (definition == null)
				{
					break;
				}

				string module = GetModuleName(context.Type);
				bool isBuiltIn = ScriptExportManager.IsBuiltinLibrary(module);
				foreach (PropertyDefinition property in definition.Properties)
				{
					MethodDefinition method = property.GetMethod == null ? property.SetMethod : property.GetMethod;
					if (method.IsVirtual && (method.IsNewSlot || method.IsReuseSlot))
					{
						for (int i = 0; i < overrides.Count; i++)
						{
							PropertyDefinition @override = overrides[i];
							if (@override.Name == property.Name)
							{
								if (isBuiltIn && method.IsAbstract)
								{
									ScriptExportProperty exportProperty = manager.RetrieveProperty(@override);
									properties.Add(exportProperty);
								}

								overrides.RemoveAt(i);
								break;
							}
						}
					}
				}
			}
			return properties.ToArray();
		}

		private IReadOnlyList<ScriptExportField> CreateFields(IScriptExportManager manager)
		{
			if (Definition == null)
			{
				return Array.Empty<ScriptExportField>();
			}

			List<ScriptExportField> fields = new List<ScriptExportField>();
			foreach (FieldDefinition field in Definition.Fields)
			{
				if (!MonoType.IsSerializableModifier(field))
				{
					continue;
				}

				// if we can't determine whether it serializable or not, then consider it as serializable
				if (IsSerializationApplicable(field.FieldType))
				{
					MonoFieldContext scope = new MonoFieldContext(field, manager.Layout);
					if (!MonoType.IsFieldTypeSerializable(scope))
					{
						continue;
					}
				}

				ScriptExportField efield = manager.RetrieveField(field);
				fields.Add(efield);
			}
			return fields.ToArray();
		}

		/// <summary>
		/// This is a hardcoded way to prevent IsFieldTypeSerializable crash
		/// </summary>
		private static bool IsSerializationApplicable(TypeReference type)
		{
			if (type.ContainsGenericParameter)
			{
				return false;
			}

			if (type.IsGenericInstance)
			{
				GenericInstanceType instance = (GenericInstanceType)type;
				type = instance.GenericArguments[0];
			}
			else if (type.IsArray)
			{
				type = type.GetElementType();
			}

			while (type != null)
			{
				if (type.Module == null)
				{
					return false;
				}
				TypeDefinition definition = type.Resolve();
				if (definition == null)
				{
					return false;
				}
				type = definition.BaseType;
			}
			return true;
		}

		public override string FullName { get; }
		public override string NestedName { get; }
		public override string CleanNestedName { get; }
		public override string TypeName { get; }
		public override string CleanName { get; }
		public override string Namespace => DeclaringType == null ? Type.Namespace : DeclaringType.Namespace;
		public override string Module { get; }

		public override ScriptExportType DeclaringType => m_declaringType;
		public override ScriptExportType Base => m_base;

		public override ScriptExportConstructor Constructor => m_constructor;
		public override IReadOnlyList<ScriptExportMethod> Methods => m_methods;
		public override IReadOnlyList<ScriptExportProperty> Properties => m_properties;
		public override IReadOnlyList<ScriptExportField> Fields => m_fields;

		public override string Keyword
		{
			get
			{
				if (Definition == null)
				{
					return PublicKeyWord;
				}

				if (Definition.IsPublic || Definition.IsNestedPublic)
				{
					return PublicKeyWord;
				}
				if (Definition.IsNestedPrivate)
				{
					return PrivateKeyWord;
				}
				if (Definition.IsNestedFamily)
				{
					return ProtectedKeyWord;
				}
				return InternalKeyWord;
			}
		}
		public override bool IsStruct => Type.IsValueType;
		public override bool IsSerializable => Definition == null ? false : Definition.IsSerializable;

		private TypeReference Type { get; }
		private TypeDefinition Definition { get; }

		private ScriptExportType m_declaringType;
		private ScriptExportType m_base;
		private ScriptExportConstructor m_constructor;
		private IReadOnlyList<ScriptExportMethod> m_methods;
		private IReadOnlyList<ScriptExportProperty> m_properties;
		private IReadOnlyList<ScriptExportField> m_fields;
	}
}
