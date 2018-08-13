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
			m_element = manager.RetrieveType(Type.GetElementType());
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

		public override string FullName => m_fullName;
		public override string Name => Type.Name;
		public override string Namespace => Type.Namespace;
		public override string Module => m_module;

		protected override ScriptExportType Element => m_element;

		private TypeReference Type { get; }

		private ScriptExportType m_element;
		private string m_fullName;
		private string m_module;
	}
}