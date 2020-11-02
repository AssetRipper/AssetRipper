using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace uTinyRipper.Converters.Script.Mono
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

			CleanName = ScriptExportMonoType.GetSimpleName(Type);
			TypeName = ScriptExportMonoType.GetTypeName(Type);
			NestedName = ScriptExportMonoType.GetNestedName(Type, TypeName);
			Module = ScriptExportMonoType.GetModuleName(Type);
			FullName = ScriptExportMonoType.GetFullName(Type, Module);
		}

		public override void Init(IScriptExportManager manager)
		{
			m_template = manager.RetrieveType(Type.ElementType);

			int argumentCount = MonoUtils.GetGenericArgumentCount(Type);
			m_arguments = new ScriptExportType[argumentCount];
			for (int i = Type.GenericArguments.Count - argumentCount, j = 0; i < Type.GenericArguments.Count; i++, j++)
			{
				TypeReference argument = Type.GenericArguments[i];
				m_arguments[j] = manager.RetrieveType(argument);
			}

			if (Type.DeclaringType != null)
			{
				if (Type.DeclaringType.HasGenericParameters)
				{
					IEnumerable<TypeReference> nestArguments = Type.GenericArguments.Take(Type.GenericArguments.Count - argumentCount);
					GenericInstanceType nestInstance = MonoUtils.CreateGenericInstance(Type.DeclaringType, nestArguments);
					m_nest = manager.RetrieveType(nestInstance);
				}
				else
				{
					m_nest = manager.RetrieveType(Type.DeclaringType);
				}
			}
		}

		public override bool HasMember(string name)
		{
			if (base.HasMember(name))
			{
				return true;
			}
			return ScriptExportMonoType.HasMember(Type.ElementType, name);
		}

		public override ScriptExportType NestType => m_nest;
		public override ScriptExportType Template => m_template;
		public override IReadOnlyList<ScriptExportType> Arguments => m_arguments;

		public override string FullName { get; }
		public override string NestedName { get; }
		public override string TypeName { get; }
		public override string CleanName { get; }
		public override string Namespace => Type.Namespace;
		public override string Module { get; }

		private GenericInstanceType Type { get; }

		private ScriptExportType m_nest;
		private ScriptExportType m_template;
		private ScriptExportType[] m_arguments;
	}
}
