using AssetRipper.Project.Exporters.Script.Elements;
using AssetRipper.Structure.Assembly.Managers;
using AssetRipper.Structure.Assembly.Mono;
using AssetRipper.Structure.Assembly.Mono.Extensions;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetRipper.Project.Exporters.Script.Mono
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

			CleanName = MonoUtils.GetSimpleName(Type);
			TypeName = MonoUtils.GetTypeName(Type);
			NestedName = MonoUtils.GetNestedName(Type, TypeName);
			CleanNestedName = MonoUtils.ToCleanName(NestedName);
			Module = MonoUtils.GetModuleName(Type);
			FullName = MonoUtils.GetFullName(Type, Module);
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
			return MonoUtils.HasMember(Type, name);
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

				string module = MonoUtils.GetModuleName(definition);
				bool isBuiltIn = MonoUtils.IsBuiltinLibrary(module);
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

				string module = MonoUtils.GetModuleName(context.Type);
				bool isBuiltIn = MonoUtils.IsBuiltinLibrary(module);
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
				if (!MonoUtils.IsSerializableModifier(field))
				{
					continue;
				}

				// if we can't determine whether it serializable or not, then consider it as serializable
				if (IsSerializationApplicable(field.FieldType))
				{
					MonoFieldContext scope = new MonoFieldContext(field, manager.Layout);
					if (!MonoUtils.IsFieldTypeSerializable(scope))
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
