using Mono.Cecil;
using System;

namespace UtinyRipper.Exporters.Scripts.Mono
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
			m_type = manager.RetrieveType(Field.FieldType);
			m_attribute = CreateAttribute(manager);

			TypeReference baseType = Field.DeclaringType.BaseType;
			while(baseType != null)
			{
				if(baseType.Module == null)
				{
					break;
				}

				TypeDefinition baseDefinition = baseType.Resolve();
				m_isNew = HasSameField(baseDefinition);
				if(m_isNew)
				{
					break;
				}

				baseType = baseDefinition.BaseType;
			}
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

		private bool HasSameField(TypeDefinition type)
		{
			foreach (FieldDefinition field in type.Fields)
			{
				if (field.Name == Name)
				{
					return true;
				}
			}

			foreach (PropertyDefinition property in type.Properties)
			{
				if (property.Name == Name)
				{
					return true;
				}
			}

			return false;
		}

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
		protected override bool IsNew => m_isNew;
		protected override string Name => Field.Name;
		protected override string Constant => Field.Constant.ToString();

		private FieldDefinition Field { get; }

		private ScriptExportType m_type;
		private ScriptExportAttribute m_attribute;
		private bool m_isNew;
	}
}
