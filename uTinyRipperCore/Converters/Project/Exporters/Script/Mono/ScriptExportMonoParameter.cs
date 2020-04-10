using Mono.Cecil;
using System;

namespace uTinyRipper.Converters.Script.Mono
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
			m_type = manager.RetrieveType(Parameter.ParameterType.IsByReference ? Parameter.ParameterType.GetElementType() : Parameter.ParameterType);
		}

		public override ScriptExportType Type => m_type;

		public override string Name => Parameter.Name;

		public override ByRefType IsByRef
		{
			get
			{
				if (!Parameter.ParameterType.IsByReference) return ByRefType.None;

				if (Parameter.IsOut) return ByRefType.Out;
				if (Parameter.IsIn) return ByRefType.In;

				return ByRefType.Ref;
			}
		}

		private ParameterDefinition Parameter { get; }

		private ScriptExportType m_type;
	}
}
