using Mono.Cecil;
using System;

namespace uTinyRipper.Converters.Script.Mono
{
	public sealed class ScriptExportMonoParameter : ScriptExportParameter
	{
		public ScriptExportMonoParameter(ParameterDefinition parameter)
		{
			Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
		}

		public override void Init(IScriptExportManager manager)
		{
			m_type = manager.RetrieveType(Parameter.ParameterType.IsByReference ? Parameter.ParameterType.GetElementType() : Parameter.ParameterType);
		}

		public override string Name => Parameter.Name;

		public override ScriptExportType Type => m_type;

		public override ByRefType ByRef
		{
			get
			{
				if (Parameter.ParameterType.IsByReference)
				{
					if (Parameter.IsOut)
					{
						return ByRefType.Out;
					}
					if (Parameter.IsIn)
					{
						return ByRefType.In;
					}
					return ByRefType.Ref;
				}
				return ByRefType.None;
			}
		}

		private ParameterDefinition Parameter { get; }

		private ScriptExportType m_type;
	}
}
