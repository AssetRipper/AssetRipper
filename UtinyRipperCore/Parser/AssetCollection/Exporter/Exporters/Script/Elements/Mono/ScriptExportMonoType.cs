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

			m_name = GetName();
			m_module = GetModule(Type);
			m_fullName = ScriptExportManager.ToFullName(Module, Type.FullName);
		}

		public static string ToFullName(TypeReference type)
		{
			return ScriptExportManager.ToFullName(GetModule(type), type.FullName);
		}

		public static string GetModule(TypeReference type)
		{
			return Path.GetFileNameWithoutExtension(type.Scope.Name);
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
				if(!Type.IsGenericParameter)
				{
					AddAsNestedType(manager);
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

		protected override bool HasMemberInner(string name)
		{
			if(Definition == null)
			{
				return false;
			}
			return HasMemberInner(Definition.BaseType, name);
		}

		private bool HasMemberInner(TypeReference type, string name)
		{
			if(type == null)
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
				if (field.Name == Name)
				{
					return true;
				}
			}
			foreach (PropertyDefinition property in definition.Properties)
			{
				if (property.Name == Name)
				{
					return true;
				}
			}
			return HasMemberInner(definition.BaseType, name);
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
				if (field.IsPublic)
				{
					if (field.IsNotSerialized)
					{
						continue;
					}
				}
				else
				{
					if (!ScriptExportMonoField.HasSerializeFieldAttribute(field))
					{
						continue;
					}
				}

				if (field.FieldType.Module == null)
				{
					// if field has unknown type then consider it as serializable
				}
				else
				{
					TypeDefinition fieldTypeDefinition = field.FieldType.Resolve();
					if(fieldTypeDefinition.IsInterface)
					{
						continue;
					}
					if (!MonoType.IsSerializableType(field.FieldType))
					{
						continue;
					}
				}

				ScriptExportField efield = manager.RetrieveField(field);
				fields.Add(efield);
			}
			return fields.ToArray();
		}

		private string GetName()
		{
			if(MonoType.IsPrimitive(Type))
			{
				return ScriptType.ToPrimitiveString(Type.Name);
			}
			if(MonoType.IsString(Type))
			{
				return ScriptType.CStringName;
			}

			string name = string.Empty;
			bool isArray = Type.IsArray;
			TypeReference elementType = Type;
			if (isArray)
			{
				elementType = Type.GetElementType();
			}

			string typeName = elementType.Name;
			if (Type.HasGenericParameters)
			{
				int index = typeName.IndexOf('`');
				if (index > 0)
				{
					string fixedName = typeName.Substring(0, index);
					name += fixedName;
					name += '<';
					if(Type.GenericParameters.Count == 1)
					{
						name += 'T';
					}
					else
					{
						for (int i = 0; i < Type.GenericParameters.Count; i++)
						{
							GenericParameter par = Type.GenericParameters[i];
							name += $"T{i}";
							if (i < Type.GenericParameters.Count - 1)
							{
								name += ", ";
							}
						}
					}
					name += '>';
				}
				else
				{
					name += typeName;
				}
			}
			else
			{
				name += typeName;
			}

			if (isArray)
			{
				name += "[]";
			}
			return name;
		}

		public override string FullName => m_fullName;
		public override string Name => m_name;
		public override string Namespace => Type.Namespace;
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

		private string m_fullName;
		private string m_name;
		private string m_module;
		private ScriptExportType m_declaringType;
		private ScriptExportType m_base;
		private IReadOnlyList<ScriptExportField> m_fields;
	}
}
