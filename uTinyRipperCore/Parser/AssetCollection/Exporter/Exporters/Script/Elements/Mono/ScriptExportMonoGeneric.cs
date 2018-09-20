using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace uTinyRipper.Exporters.Scripts.Mono
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

			TypeName = ScriptExportMonoType.GetTypeName(Type);
			Name = ScriptExportMonoType.GetName(Type, TypeName);
			Module = ScriptExportMonoType.GetModule(Type);
			FullName = ScriptExportMonoType.GetFullName(Type, Module);
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

		public override ScriptExportType Template => m_owner;
		public override IReadOnlyList<ScriptExportType> Arguments => m_arguments;
		public override ScriptExportType Base => Template.Base;

		public override string FullName { get; }
		public override string Name { get; }
		public override string TypeName { get; }
		public override string Namespace => Type.Namespace;
		public override string Module { get; }

		private GenericInstanceType Type { get; }

		private ScriptExportType m_owner;
		private ScriptExportType[] m_arguments;
	}
}
