using Mono.Cecil;
using System;
using System.Collections.Generic;

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

		private static List<CustomAttribute> GetExportAttributes(FieldDefinition field)
		{
			if (field.CustomAttributes.Count == 0)
			{
				return null;
			}

			List<CustomAttribute> attributes = new List<CustomAttribute>();
			foreach (CustomAttribute attr in field.CustomAttributes)
			{
				if (ScriptExportMonoAttribute.IsSerializeFieldAttribute(attr) ||
					ScriptExportMonoAttribute.IsMulitlineAttribute(attr) ||
					ScriptExportMonoAttribute.IsTextAreaAttribute(attr))
				{
					attributes.Add(attr);
				}
			}
			return attributes;
		}

		public override void Init(IScriptExportManager manager)
		{
			m_declaringType = manager.RetrieveType(Field.DeclaringType);
			m_type = manager.RetrieveType(Field.FieldType);
			m_attributes = CreateAttributes(manager);
		}

		private ScriptExportAttribute[] CreateAttributes(IScriptExportManager manager)
		{
			List<CustomAttribute> attributes = GetExportAttributes(Field);
			if (attributes == null || attributes.Count == 0)
			{
				return null;
			}
			else
			{
				ScriptExportAttribute[] result = new ScriptExportAttribute[attributes.Count];
				for (int i = 0; i < attributes.Count; i++)
				{
					result[i] = manager.RetrieveAttribute(attributes[i]);
				}
				return result;
			}
		}

		public override string Name => Field.Name;

		public override ScriptExportType DeclaringType => m_declaringType;
		public override ScriptExportType Type => m_type;
		public override IReadOnlyList<ScriptExportAttribute> Attributes => m_attributes;

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
		private ScriptExportAttribute[] m_attributes;
	}
}
