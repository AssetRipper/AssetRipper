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

			m_name = GetName();
			m_module = ScriptExportMonoType.GetModule(Type);
			m_fullName = ScriptExportManager.ToFullName(Module, Type.FullName);
		}

		public override void Init(IScriptExportManager manager)
		{
			TypeSpecification specification = (TypeSpecification)Type;
			m_owner = manager.RetrieveType(specification.ElementType);

			List<ScriptExportType> arguments = new List<ScriptExportType>();
			foreach(TypeReference argument in Type.GenericArguments)
			{
				ScriptExportType exportParameter = manager.RetrieveType(argument);
				arguments.Add(exportParameter);
			}
			m_arguments = arguments.ToArray();
		}

		private string GetName()
		{
			string name = string.Empty;
			bool isArray = Type.IsArray;
			TypeReference elementType = Type;
			if (isArray)
			{
				elementType = Type.GetElementType();
			}
	
			if (Type.IsGenericInstance)
			{
				int index = elementType.Name.IndexOf('`');
				if (index > 0)
				{
					string fixedName = elementType.Name.Substring(0, index);
					name += fixedName;
					name += '<';
					GenericInstanceType generic = (GenericInstanceType)Type;
					for (int i = 0; i < generic.GenericArguments.Count; i++)
					{
						TypeReference arg = generic.GenericArguments[i];
						name += arg.Name;
						if (i < generic.GenericArguments.Count - 1)
						{
							name += ", ";
						}
					}
					name += '>';
				}
				else
				{
					name += elementType.Name;
				}
			}
			else
			{
				name += elementType.Name;
			}

			if (isArray)
			{
				name += "[]";
			}
			return name;
		}

		public override ScriptExportType Owner => m_owner;
		public override IReadOnlyList<ScriptExportType> Arguments => m_arguments;
		public override ScriptExportType Base => Owner.Base;

		public override string FullName => m_fullName;
		public override string Name => m_name;
		public override string Namespace => Type.Namespace;
		public override string Module => m_module;

		private GenericInstanceType Type { get; }

		private ScriptExportType m_owner;
		private ScriptExportType[] m_arguments;
		private string m_fullName;
		private string m_name;
		private string m_module;
	}
}
