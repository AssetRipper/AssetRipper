using Mono.Cecil;
using System;

namespace uTinyRipper.Exporters.Scripts.Mono
{
	public sealed class ScriptExportMonoProperty: ScriptExportProperty
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
			m_propertyType = manager.RetrieveType(Property.PropertyType);
		}

		public override string Name => Property.Name;
		protected override bool IsOverride => true;
		protected override string GetKeyword
		{
			get
			{
				if (!HasGetter)
				{
					return "";
				}
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
					return ProtectedKeyWord;
				}
				return ProtectedKeyWord;
			}
		}
		protected override string SetKeyword
		{
			get
			{
				if(!HasSetter)
				{
					return "";
				}
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
					return ProtectedKeyWord;
				}
				return ProtectedKeyWord;
			}
		}

		protected override bool HasGetter => Property.GetMethod != null;
		protected override bool HasSetter => Property.SetMethod != null;
		public override ScriptExportType DeclaringType => m_declaringType;
		public override ScriptExportType PropertyType => m_propertyType;


		private PropertyDefinition Property { get; }

		private ScriptExportType m_declaringType;
		private ScriptExportType m_propertyType;
	}
}
