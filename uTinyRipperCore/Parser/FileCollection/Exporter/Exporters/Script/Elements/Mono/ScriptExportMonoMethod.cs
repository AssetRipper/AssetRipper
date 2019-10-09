using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace uTinyRipper.Exporters.Scripts.Mono
{
	public sealed class ScriptExportMonoMethod : ScriptExportMethod
	{
		public ScriptExportMonoMethod(MethodDefinition method)
		{
			if (method == null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			Method = method;
		}

		public override void Init(IScriptExportManager manager)
		{
			m_declaringType = manager.RetrieveType(Method.DeclaringType);
			m_returnType = manager.RetrieveType(Method.ReturnType);
			int argumentCount = Method.Parameters.Count;
			m_parameters = new ScriptExportParameter[argumentCount];
			for (int i = 0; i < argumentCount; i++)
			{
				ParameterDefinition argument = Method.Parameters[i];
				m_parameters[i] = manager.RetrieveParameter(argument);
			}
		}

		public override string Name => Method.IsConstructor ? Method.DeclaringType.Name : Method.Name;
		protected override bool IsOverride => true;
		protected override string Keyword
		{
			get
			{
				if (Method.IsPublic)
				{
					return PublicKeyWord;
				}
				if (Method.IsPrivate)
				{
					return PrivateKeyWord;
				}
				if (Method.IsAssembly)
				{
					return ProtectedKeyWord;
				}
				return ProtectedKeyWord;
			}
		}

		public override ScriptExportType DeclaringType => m_declaringType;
		public override ScriptExportType ReturnType => m_returnType;
		public override IReadOnlyList<ScriptExportParameter> Parameters => m_parameters;


		private MethodDefinition Method { get; }

		private ScriptExportType m_declaringType;
		private ScriptExportParameter[] m_parameters;
		private ScriptExportType m_returnType;
	}
}
