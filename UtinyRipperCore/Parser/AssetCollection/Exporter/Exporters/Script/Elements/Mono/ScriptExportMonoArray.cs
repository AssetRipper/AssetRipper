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
			m_fullName = ScriptExportManager.ToFullName(Module, Type.FullName);
		}
		
		public override void Init(IScriptExportManager manager)
		{
			TypeSpecification specification = (TypeSpecification)Type;
			m_element = manager.RetrieveType(specification.ElementType);
		}

		protected override bool HasMemberInner(string name)
		{
			throw new NotSupportedException();
		}

		public override ScriptExportType Element => m_element;

		public override string FullName => m_fullName;
		public override string Name => Type.Name;
		public override string Namespace => Type.Namespace;
		public override string Module => m_module;

		private TypeReference Type { get; }

		private ScriptExportType m_element;
		private string m_fullName;
		private string m_module;
	}
}