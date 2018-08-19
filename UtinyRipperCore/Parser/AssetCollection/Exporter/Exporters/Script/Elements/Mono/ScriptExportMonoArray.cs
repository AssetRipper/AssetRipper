using Mono.Cecil;
using System;

namespace UtinyRipper.Exporters.Scripts.Mono
{
	public sealed class ScriptExportMonoArray : ScriptExportArray
	{
		public ScriptExportMonoArray(TypeReference type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}
			if (!type.IsArray)
			{
				throw new Exception("Type isn't an array");
			}

			Type = type;

			m_module = ScriptExportMonoType.GetModule(Type);
			m_name = ScriptExportMonoType.GetName(Type);
			m_fullName = ScriptExportMonoType.ToFullName(Type, Module);
		}
		
		public override void Init(IScriptExportManager manager)
		{
			TypeSpecification specification = (TypeSpecification)Type;
			m_element = manager.RetrieveType(specification.ElementType);
		}

		public sealed override bool HasMember(string name)
		{
			throw new NotSupportedException();
		}

		public override ScriptExportType Element => m_element;

		public override string FullName => m_fullName;
		public override string Name => m_name;
		public override string Namespace => Element.Namespace;
		public override string Module => m_module;

		private TypeReference Type { get; }

		private readonly string m_fullName;
		private readonly string m_name;
		private readonly string m_module;

		private ScriptExportType m_element;
	}
}