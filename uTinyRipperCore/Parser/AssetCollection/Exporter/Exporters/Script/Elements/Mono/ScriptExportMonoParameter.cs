using Mono.Cecil;
using System;

namespace uTinyRipper.Exporters.Scripts.Mono
{
	public sealed class ScriptExportMonoParameter : ScriptExportParameter
	{
		public ScriptExportMonoParameter(ParameterDefinition parameter)
		{
			if (parameter == null)
			{
				throw new ArgumentNullException(nameof(parameter));
			}
			
			Parameter = parameter;
		}

		public override void Init(IScriptExportManager manager)
		{
			m_type = manager.RetrieveType(Parameter.ParameterType);
		}

		protected override ScriptExportType Type => m_type;

		protected override string Name => Parameter.Name;

		private ParameterDefinition Parameter { get; }

		private ScriptExportType m_type;
	}
}
