using System;
using System.Collections.Generic;

namespace uTinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportArray : ScriptExportType
	{
		public override void GetUsedNamespaces(ICollection<string> namespaces)
		{
			Element.GetTypeNamespaces(namespaces);
		}

		public sealed override string CleanNestedName => Element.CleanNestedName;

		public sealed override ScriptExportType DeclaringType => Element.DeclaringType;
		public sealed override ScriptExportType Base => Element.Base;
		public abstract ScriptExportType Element { get; }

		public sealed override IReadOnlyList<ScriptExportField> Fields { get; } = new ScriptExportField[0];

		protected sealed override string Keyword => throw new NotSupportedException();

		protected sealed override bool IsStruct => throw new NotSupportedException();
		protected sealed override bool IsSerializable => throw new NotSupportedException();
	}
}