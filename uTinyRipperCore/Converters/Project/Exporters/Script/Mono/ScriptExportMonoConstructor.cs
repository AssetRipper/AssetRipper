using System.Collections.Generic;

namespace uTinyRipper.Converters.Script.Mono
{
	public sealed class ScriptExportMonoConstructor : ScriptExportConstructor
	{
		public ScriptExportMonoConstructor(ScriptExportMethod method, ScriptExportMonoMethod baseMethod)
		{
			m_method = method;
			m_baseMethod = baseMethod;
		}

		public override void Init(IScriptExportManager manager)
		{
			m_method.Init(manager);
			m_baseMethod?.Init(manager);
		}

		public override string Name => m_method.Name;

		public override ScriptExportType DeclaringType => m_method.DeclaringType;

		public override ScriptExportType ReturnType => m_method.ReturnType;

		public override IReadOnlyList<ScriptExportParameter> Parameters => m_method.Parameters;

		public override IReadOnlyList<ScriptExportParameter> BaseParameters => m_baseMethod?.Parameters;

		public override string Keyword => m_method.Keyword;

		private readonly ScriptExportMethod m_method;
		private readonly ScriptExportMethod m_baseMethod;
	}
}
