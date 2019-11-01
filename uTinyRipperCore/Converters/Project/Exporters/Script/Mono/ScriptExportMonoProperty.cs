using Mono.Cecil;
using System;

namespace uTinyRipper.Converters.Script.Mono
{
	public sealed class ScriptExportMonoProperty : ScriptExportProperty
	{
		public ScriptExportMonoProperty(PropertyDefinition property)
		{
			if (property == null)
			{
				throw new ArgumentNullException(nameof(property));
			}

			Property = property;
		}

		public override void Init(IScriptExportManager manager)
		{
			m_declaringType = manager.RetrieveType(Property.DeclaringType);
			m_type = manager.RetrieveType(Property.PropertyType);
		}

		public override string Name => Property.Name;

		public override ScriptExportType DeclaringType => m_declaringType;
		public override ScriptExportType Type => m_type;

		protected override bool HasGetter => Property.GetMethod != null;
		protected override bool HasSetter => Property.SetMethod != null;

		protected override string GetKeyword
		{
			get
			{
				if (Property.GetMethod.IsPublic)
				{
					return PublicKeyWord;
				}
				if (Property.GetMethod.IsPrivate)
				{
					return PrivateKeyWord;
				}
				if (Property.GetMethod.IsAssembly)
				{
					return InternalKeyWord;
				}
				return ProtectedKeyWord;
			}
		}

		protected override string SetKeyword
		{
			get
			{
				if (Property.SetMethod.IsPublic)
				{
					return PublicKeyWord;
				}
				if (Property.SetMethod.IsPrivate)
				{
					return PrivateKeyWord;
				}
				if (Property.SetMethod.IsAssembly)
				{
					return InternalKeyWord;
				}
				return ProtectedKeyWord;
			}
		}

		private PropertyDefinition Property { get; }

		private ScriptExportType m_declaringType;
		private ScriptExportType m_type;
	}
}
