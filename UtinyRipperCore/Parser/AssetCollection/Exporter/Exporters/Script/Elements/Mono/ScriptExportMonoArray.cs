using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.Exporters.Scripts.Mono
{
	public sealed class ScriptExportMonoArray : ScriptExportArray
	{
		public ScriptExportMonoArray(TypeReference @type, TypeDefinition @definition)
		{
			if (@type == null || @definition == null)
			{
				throw new ArgumentNullException(nameof(@type));
			}
			if (!type.IsArray)
			{
				throw new Exception("Type isn't an array");
			}

			Type = @type;
			Definition = @definition;
		}

		public override void Init(IScriptExportManager manager)
		{
			m_element = CreateElementType(manager);

			m_module = Path.GetFileNameWithoutExtension(Type.Scope.Name);
			m_fullName = $"[{Module}]{Type.FullName}";
		}


		public override ScriptExportType GetContainer(IScriptExportManager manager)
		{
			if (Type.IsNested)
			{
				return manager.RetrieveType(Type.DeclaringType);
			}
			else
			{
				return this;
			}
		}


		private ScriptExportType CreateElementType(IScriptExportManager manager)
		{
			return manager.RetrieveType(Type.GetElementType());
		}


		protected override string Keyword
		{
			get
			{
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


		public override string FullName => m_fullName;
		public override string Name => Type.Name;
		public override string Namespace => Type.Namespace;
		public override string Module => m_module;

		protected override ScriptExportType Element => m_element;

		private TypeReference Type { get; }
		private TypeDefinition Definition { get; }

		private ScriptExportType m_element;
		private string m_fullName;
		private string m_module;
	}
}