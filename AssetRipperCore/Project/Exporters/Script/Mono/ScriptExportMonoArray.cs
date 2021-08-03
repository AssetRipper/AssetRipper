using AssetRipper.Core.Project.Exporters.Script.Elements;
using AssetRipper.Core.Structure.Assembly.Mono;
using Mono.Cecil;
using System;

namespace AssetRipper.Core.Project.Exporters.Script.Mono
{
	public sealed class ScriptExportMonoArray : ScriptExportArray
	{
		public ScriptExportMonoArray(TypeReference type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}
			if (!type.IsArray)
			{
				throw new Exception("Type isn't an array");
			}

			Type = type;

			CleanName = MonoUtils.GetSimpleName(Type);
			TypeName = MonoUtils.GetTypeName(Type);
			NestedName = MonoUtils.GetNestedName(Type, TypeName);
			Module = MonoUtils.GetModuleName(Type);
			FullName = MonoUtils.GetFullName(Type, Module);
		}

		public override void Init(IScriptExportManager manager)
		{
			TypeSpecification specification = (TypeSpecification)Type;
			m_element = manager.RetrieveType(specification.ElementType);
		}

		public sealed override bool HasMember(string name)
		{
			throw new NotSupportedException();
		}

		public override ScriptExportType Element => m_element;

		public override string FullName { get; }
		public override string CleanName { get; }
		public override string NestedName { get; }
		public override string TypeName { get; }
		public override string Namespace => Element.Namespace;
		public override string Module { get; }

		private TypeReference Type { get; }

		private ScriptExportType m_element;
	}
}