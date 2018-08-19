using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace UtinyRipper.Exporters.Scripts.Mono
{
	public sealed class ScriptExportMonoGeneric : ScriptExportGeneric
	{
		public ScriptExportMonoGeneric(TypeReference type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}
			if (!type.IsGenericInstance)
			{
				throw new Exception("Type isn't generic");
			}

			Type = (GenericInstanceType)type;

			m_name = ScriptExportMonoType.GetName(type);
			m_module = ScriptExportMonoType.GetModule(Type);
			m_fullName = ScriptExportMonoType.ToFullName(Type, Module);
		}

		public override void Init(IScriptExportManager manager)
		{
			m_owner = manager.RetrieveType(Type.ElementType);

			List<ScriptExportType> arguments = new List<ScriptExportType>();
			foreach(TypeReference argument in Type.GenericArguments)
			{
				ScriptExportType exportParameter = manager.RetrieveType(argument);
				arguments.Add(exportParameter);
			}
			m_arguments = arguments.ToArray();
		}

		public override bool HasMember(string name)
		{
			if (base.HasMember(name))
			{
				return true;
			}
			return ScriptExportMonoType.HasMember(Type.ElementType, name);
		}

		public override ScriptExportType Owner => m_owner;
		public override IReadOnlyList<ScriptExportType> Arguments => m_arguments;
		public override ScriptExportType Base => Owner.Base;

		public override string FullName => m_fullName;
		public override string Name => m_name;
		public override string Namespace => Type.Namespace;
		public override string Module => m_module;

		private GenericInstanceType Type { get; }

		private readonly string m_fullName;
		private readonly string m_name;
		private readonly string m_module;

		private ScriptExportType m_owner;
		private ScriptExportType[] m_arguments;
	}
}
