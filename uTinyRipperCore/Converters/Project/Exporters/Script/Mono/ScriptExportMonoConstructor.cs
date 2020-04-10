using System.Collections.Generic;

namespace uTinyRipper.Converters.Script.Mono
{
	public sealed class ScriptExportMonoConstructor : ScriptExportMethod
	{
		public ScriptExportMonoConstructor(ScriptExportMethod method, ScriptExportType declaringType)
		{
			m_method = method;
			m_declaringType = declaringType;
		}

		public override void Init(IScriptExportManager manager)
		{
			m_method.Init(manager);
		}

		public override string Name => m_method.Name;

		public override ScriptExportType DeclaringType => m_declaringType;

		public override ScriptExportType ReturnType => m_method.ReturnType;

		public override IReadOnlyList<ScriptExportParameter> Parameters => m_method.Parameters;

		public override string Keyword => m_method.Keyword;

		private readonly ScriptExportMethod m_method;
		private readonly ScriptExportType m_declaringType;
	}
}
