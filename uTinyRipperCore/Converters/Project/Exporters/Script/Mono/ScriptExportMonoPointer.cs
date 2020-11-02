using Mono.Cecil;
using System;

namespace uTinyRipper.Converters.Script.Mono
{
	public sealed class ScriptExportMonoPointer : ScriptExportPointer
	{
		public ScriptExportMonoPointer(TypeReference type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}
			if (!type.IsPointer)
			{
				throw new Exception("Type isn't a pointer");
			}

			Type = type;

			CleanName = ScriptExportMonoType.GetSimpleName(Type);
			TypeName = ScriptExportMonoType.GetTypeName(Type);
			NestedName = ScriptExportMonoType.GetNestedName(Type, TypeName);
			Module = ScriptExportMonoType.GetModuleName(Type);
			FullName = ScriptExportMonoType.GetFullName(Type, Module);
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