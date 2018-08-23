using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.AssetExporters.Mono;

namespace UtinyRipper.Exporters.Scripts.Mono
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

			m_name = GetName(Type);
			m_module = GetModule(Type);
			m_fullName = ToFullName(Type, Module);
		}

		public static string ToFullName(TypeReference type)
		{
			string module = GetModule(type);
			return ToFullName(type, module);
		}

		public static string ToFullName(TypeReference type, string module)
		{
			string name = GetName(type);
			string fullName = $"{type.Namespace}.{ToNestedName(type, name)}";
			return ScriptExportManager.ToFullName(module, fullName);
		}

		public static string GetModule(TypeReference type)
		{
			return Path.GetFileNameWithoutExtension(type.Scope.Name);
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

		public static string GetName(TypeReference type)
		{
			if (MonoType.IsCPrimitive(type))
			{
				return ScriptType.ToCPrimitiveString(type.Name);
			}

			string name = type.Name;
			if (type.IsGenericInstance)
			{
				GenericInstanceType generic = (GenericInstanceType)type;
				int index = name.IndexOf('`');
				name = name.Substring(0, index);
				name += '<';
				for (int i = 0; i < generic.GenericArguments.Count; i++)
				{
					TypeReference arg = generic.GenericArguments[i];
					name += GetArgumentName(arg);
					if (i < generic.GenericArguments.Count - 1)
					{
						name += ", ";
					}
				}
				name += '>';
			}
			else if (type.HasGenericParameters)
			{
				int index = name.IndexOf('`');
				name = name.Substring(0, index);
				name += '<';
				for (int i = 0; i < type.GenericParameters.Count; i++)
				{
					GenericParameter par = type.GenericParameters[i];
					name += GetArgumentName(par);
					if (i < type.GenericParameters.Count - 1)
					{
						name += ", ";
					}
				}
				name += '>';
			}
			else if(type.IsArray)
			{
				ArrayType array = (ArrayType)type;
				name = GetName(array.ElementType) + $"[{new string(',', array.Dimensions.Count - 1)}]";
			}
			return name;
		}

		private static string GetArgumentName(TypeReference type)
		{
			if(MonoType.IsEngineObject(type))
			{
				return $"{type.Namespace}.{type.Name}";
			}

			string name = GetName(type);
			return ToNestedName(type, name);
		}

		private static string ToNestedName(TypeReference type, string name)
		{
			if(type.IsGenericParameter)
			{
				return name;
			}
			if(type.IsNested)
			{
				string declaringName = GetName(type.DeclaringType);
				string declaringNestedName = ToNestedName(type.DeclaringType, declaringName);
				return $"{declaringNestedName}.{name}";
			}
			return name;
		}

		public override void Init(IScriptExportManager manager)
		{
			if (Definition != null && Definition.BaseType != null)
			{
				m_base = manager.RetrieveType(Definition.BaseType);
			}

			m_fields = CreateFields(manager);

			if(Type.IsNested)
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
			if(Definition != null)
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
			if(base.HasMember(name))
			{
				return true;
			}
			return HasMember(Type, name);
		}

		private IReadOnlyList<ScriptExportField> CreateFields(IScriptExportManager manager)
		{
			if(Definition == null)
			{
				return new ScriptExportField[0];
			}

			List<ScriptExportField> fields = new List<ScriptExportField>();
			foreach (FieldDefinition field in Definition.Fields)
			{
				if (field.FieldType.Module == null)
				{
					// if field has unknown type then consider it as serializable
				}
				else if(IsContainsGenericParameter(field.FieldType))
				{
					// if field type has generic parameter then consider it as serializable
				}
				else if (!MonoField.IsSerializable(field, null))
				{
					continue;
				}

				ScriptExportField efield = manager.RetrieveField(field);
				fields.Add(efield);
			}
			return fields.ToArray();
		}

		private static bool IsContainsGenericParameter(TypeReference type)
		{
			if(type.IsGenericParameter)
			{
				return true;
			}
			if(type.IsGenericInstance)
			{
				GenericInstanceType instance = (GenericInstanceType)type;
				foreach(TypeReference argument in instance.GenericArguments)
				{
					if(IsContainsGenericParameter(argument))
					{
						return true;
					}
				}
			}
			return false;
		}

		public override string FullName => m_fullName;
		public override string Name => m_name;
		public override string Namespace => DeclaringType == null ? Type.Namespace : DeclaringType.Namespace;
		public override string Module => m_module;

		public override ScriptExportType DeclaringType => m_declaringType;
		public override ScriptExportType Base => m_base;

		public override IReadOnlyList<ScriptExportField> Fields => m_fields;

		protected override string Keyword
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
		protected override bool IsStruct => Type.IsValueType;
		protected override bool IsSerializable => Definition == null ? false : Definition.IsSerializable;

		private TypeReference Type { get; }
		private TypeDefinition Definition { get; }

		private readonly string m_fullName;
		private readonly string m_name;
		private readonly string m_module;

		private ScriptExportType m_declaringType;
		private ScriptExportType m_base;
		private IReadOnlyList<ScriptExportField> m_fields;
	}
}
