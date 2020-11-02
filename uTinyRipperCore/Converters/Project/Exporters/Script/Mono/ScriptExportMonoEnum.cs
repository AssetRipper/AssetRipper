using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace uTinyRipper.Converters.Script.Mono
{
	public sealed class ScriptExportMonoEnum : ScriptExportEnum
	{
		public ScriptExportMonoEnum(TypeReference @enum)
		{
			if (@enum == null)
			{
				throw new ArgumentNullException(nameof(@enum));
			}

			Type = @enum;
			if (@enum.Module != null)
			{
				Definition = @enum.Resolve();
			}

			NestedName = ScriptExportMonoType.GetNestedName(Type);
			CleanNestedName = ScriptExportMonoType.ToCleanName(NestedName);
			Module = ScriptExportMonoType.GetModuleName(Type);
			FullName = ScriptExportMonoType.GetFullName(Type, Module);
		}

		public override void Init(IScriptExportManager manager)
		{
			if (Type.Module == null)
			{
				m_fields = Array.Empty<ScriptExportField>();
			}
			else
			{
				m_fields = CreateFields(manager);
			}

			if (Type.IsNested)
			{
				m_declaringType = manager.RetrieveType(Type.DeclaringType);
				AddAsNestedEnum();
			}
		}

		private IReadOnlyList<ScriptExportField> CreateFields(IScriptExportManager manager)
		{
			List<ScriptExportField> fields = new List<ScriptExportField>();
			foreach (FieldDefinition field in Definition.Fields)
			{
				if (field.Name == "value__")
				{
					m_base = manager.RetrieveType(field.FieldType);
					continue;
				}

				ScriptExportField enumField = manager.RetrieveField(field);
				fields.Add(enumField);
			}
			return fields;
		}


		public override ScriptExportType Base => m_base;
		public override string CleanName => Type.Name;
		public override string TypeName => Type.Name;
		public override string CleanNestedName { get; }
		public override string NestedName { get; }
		public override string FullName { get; }
		public override string Namespace => DeclaringType == null ? Type.Namespace : DeclaringType.Namespace;
		public override string Module { get; }

		public override ScriptExportType DeclaringType => m_declaringType;
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

		private TypeReference Type { get; }
		private TypeDefinition Definition { get; }

		private ScriptExportType m_declaringType;
		private ScriptExportType m_base;
		private IReadOnlyList<ScriptExportField> m_fields;
	}
}
