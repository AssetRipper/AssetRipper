using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace uTinyRipper.Exporters.Scripts.Mono
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
				m_fields = new ScriptExportField[0];
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
					continue;
				}

				ScriptExportField enumField = manager.RetrieveField(field);
				fields.Add(enumField);
			}
			return fields;
		}

		public override string NestedName { get; }
		public override string CleanNestedName { get; }
		public override string TypeName => Type.Name;
		public override string FullName { get; }
		public override string Namespace => DeclaringType == null ? Type.Namespace : DeclaringType.Namespace;
		public override string Module { get; }

		public override ScriptExportType DeclaringType => m_declaringType;
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

		private TypeReference Type { get; }
		private TypeDefinition Definition { get; }

		private ScriptExportType m_declaringType;
		private IReadOnlyList<ScriptExportField> m_fields;
	}
}
