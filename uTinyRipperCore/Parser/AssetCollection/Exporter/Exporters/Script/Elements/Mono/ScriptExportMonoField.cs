using Mono.Cecil;
using System;

namespace uTinyRipper.Exporters.Scripts.Mono
{
	public sealed class ScriptExportMonoField : ScriptExportField
	{
		public ScriptExportMonoField(FieldDefinition field)
		{
			if (field == null)
			{
				throw new ArgumentNullException(nameof(field));
			}
			
			Field = field;
		}

		public static bool HasSerializeFieldAttribute(FieldDefinition field)
		{
			return GetSerializeFieldAttribute(field) != null;
		}

		private static CustomAttribute GetSerializeFieldAttribute(FieldDefinition field)
		{
			foreach (CustomAttribute attr in field.CustomAttributes)
			{
				if (ScriptExportMonoAttribute.IsSerializeFieldAttribute(attr))
				{
					return attr;
				}
			}
			return null;
		}

		public override void Init(IScriptExportManager manager)
		{
			m_declaringType = manager.RetrieveType(Field.DeclaringType);
			m_type = manager.RetrieveType(Field.FieldType);
			m_attribute = CreateAttribute(manager);
		}

		private ScriptExportAttribute CreateAttribute(IScriptExportManager manager)
		{
			CustomAttribute attribute = GetSerializeFieldAttribute(Field);
			if (attribute == null)
			{
				return null;
			}
			else
			{
				return manager.RetrieveAttribute(attribute);
			}
		}

		private bool HasSameField(ScriptExportType type)
		{

			return false;
		}

		public override string Name => Field.Name;

		public override ScriptExportType DeclaringType => m_declaringType;
		public override ScriptExportType Type => m_type;
		public override ScriptExportAttribute Attribute => m_attribute;

		protected override string Keyword
		{
			get
			{
				if (Field.IsPublic)
				{
					return PublicKeyWord;
				}
				if (Field.IsPrivate)
				{
					return PrivateKeyWord;
				}
				if (Field.IsAssembly)
				{
					return ProtectedKeyWord;
				}
				return ProtectedKeyWord;
			}
		}
		protected override string Constant => Field.Constant.ToString();

		private FieldDefinition Field { get; }

		private ScriptExportType m_declaringType;
		private ScriptExportType m_type;
		private ScriptExportAttribute m_attribute;
	}
}
