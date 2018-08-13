using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
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

			m_genericArguments = CreateGenericArguments(manager);
			m_nestedTypes = CreateNestedTypes(manager);
			m_nestedEnums = CreateNestedEnums(manager);
			m_delegates = CreateDelegates(manager);
			m_fields = CreateFields(manager);

			// force manager to create container type
			GetContainer(manager);
		}

		public override ScriptExportType GetContainer(IScriptExportManager manager)
		{
			if(Type.IsNested)
			{
				return manager.RetrieveType(Type.DeclaringType);
			}
			else
			{
				return this;
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

		private IReadOnlyList<ScriptExportType> CreateGenericArguments(IScriptExportManager manager)
		{
			List<ScriptExportType> arguments = new List<ScriptExportType>();
			if (Type.IsGenericInstance)
			{
				GenericInstanceType generic = (GenericInstanceType)Type;
				foreach (TypeReference arg in generic.GenericArguments)
				{
					ScriptExportType argType = manager.RetrieveType(arg);
					arguments.Add(argType);
				}
			}
			return arguments.ToArray();
		}

		private IReadOnlyList<ScriptExportType> CreateNestedTypes(IScriptExportManager manager)
		{
			if(Definition == null)
			{
				return new ScriptExportType[0];
			}

			List<ScriptExportType> nestedTypes = new List<ScriptExportType>();
			foreach (TypeDefinition nested in Definition.NestedTypes)
			{
				if(nested.IsEnum)
				{
					continue;
				}
				if (!nested.IsSerializable)
				{
					continue;
				}
				if(ScriptExportMonoAttribute.IsCompilerGenerated(nested))
				{
					continue;
				}

				ScriptExportType nestedType = manager.RetrieveType(nested);
				nestedTypes.Add(nestedType);
			}
			return nestedTypes.ToArray();
		}

		private IReadOnlyList<ScriptExportEnum> CreateNestedEnums(IScriptExportManager manager)
		{
			if(Definition == null)
			{
				return new ScriptExportEnum[0];
			}

			List<ScriptExportEnum> nestedEnums = new List<ScriptExportEnum>();
			foreach (TypeDefinition nested in Definition.NestedTypes)
			{
				if (nested.IsEnum)
				{
					ScriptExportEnum nestedEnum = manager.RetrieveEnum(nested);
					nestedEnums.Add(nestedEnum);
				}
			}
			return nestedEnums.ToArray();
		}

		private IReadOnlyList<ScriptExportDelegate> CreateDelegates(IScriptExportManager manager)
		{
			if(Definition == null)
			{
				return new ScriptExportDelegate[0];
			}

			List<ScriptExportDelegate> delegates = new List<ScriptExportDelegate>();
			foreach (TypeDefinition nested in Definition.NestedTypes)
			{
				if (ScriptExportMonoDelegate.IsDelegate(nested))
				{
					ScriptExportDelegate @delegate = manager.RetrieveDelegate(nested);
					delegates.Add(@delegate);
				}
			}
			return delegates.ToArray();
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
			string name = string.Empty;
			bool isArray = Type.IsArray;
			TypeReference elementType = Type;
			if (isArray)
			{
				elementType = Type.GetElementType();
			}

			string typeName = elementType.Name;
			/*if (elementType.IsByReference)
			{
				name += "ref ";
				typeName = typeName.Substring(0, typeName.Length - 1);
			}*/

			if (Type.IsGenericInstance)
			{
				int index = typeName.IndexOf('`');
				if (index > 0)
				{
					string fixedName = typeName.Substring(0, index);
					name += fixedName;
					name += '<';
					GenericInstanceType generic = (GenericInstanceType)Type;
					for (int i = 0; i < generic.GenericArguments.Count; i++)
					{
						TypeReference arg = generic.GenericArguments[i];
						name += arg.Name;
						if (i < generic.GenericArguments.Count - 1)
						{
							name += ", ";
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

		public override IReadOnlyList<ScriptExportType> GenericArguments => m_genericArguments;
		public override IReadOnlyList<ScriptExportType> NestedTypes => m_nestedTypes;
		public override IReadOnlyList<ScriptExportEnum> NestedEnums => m_nestedEnums;
		public override IReadOnlyList<ScriptExportDelegate> Delegates => m_delegates;
		public override IReadOnlyList<ScriptExportField> Fields => m_fields;

		protected override ScriptExportType Base => m_base;

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
		private IReadOnlyList<ScriptExportType> m_genericArguments;
		private IReadOnlyList<ScriptExportType> m_nestedTypes;
		private IReadOnlyList<ScriptExportEnum> m_nestedEnums;
		private IReadOnlyList<ScriptExportDelegate> m_delegates;
		private IReadOnlyList<ScriptExportField> m_fields;
		private ScriptExportType m_base;
	}
}
