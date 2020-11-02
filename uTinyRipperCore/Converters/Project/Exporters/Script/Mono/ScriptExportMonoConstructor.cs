using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace uTinyRipper.Converters.Script.Mono
{
	public sealed class ScriptExportMonoConstructor : ScriptExportConstructor
	{
		public ScriptExportMonoConstructor(MethodDefinition method)
		{
			Method = method ?? throw new ArgumentNullException(nameof(method));
		}

		public override void Init(IScriptExportManager manager)
		{
			m_declaringType = manager.RetrieveType(Method.DeclaringType);
			m_returnType = manager.RetrieveType(Method.ReturnType);
			m_parameters = new ScriptExportParameter[Method.Parameters.Count];
			for (int i = 0; i < m_parameters.Length; i++)
			{
				ParameterDefinition argument = Method.Parameters[i];
				m_parameters[i] = manager.RetrieveParameter(argument);
			}
		}

		public override string Name => Method.Name;

		public override ScriptExportType DeclaringType => m_declaringType;
		public override ScriptExportType ReturnType => m_returnType;
		public override IReadOnlyList<ScriptExportParameter> Parameters => m_parameters;

		public override string Keyword
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
					return InternalKeyWord;
				}
				return ProtectedKeyWord;
			}
		}

		private MethodDefinition Method { get; }

		private ScriptExportType m_declaringType;
		private ScriptExportParameter[] m_parameters;
		private ScriptExportType m_returnType;
	}
}
