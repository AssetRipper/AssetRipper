using System;
using System.Collections.Generic;

namespace uTinyRipper.Converters.Script
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

		public sealed override ScriptExportConstructor Constructor => null;
		public sealed override IReadOnlyList<ScriptExportMethod> Methods { get; } = Array.Empty<ScriptExportMethod>();
		public sealed override IReadOnlyList<ScriptExportProperty> Properties { get; } = Array.Empty<ScriptExportProperty>();
		public sealed override IReadOnlyList<ScriptExportField> Fields { get; } = Array.Empty<ScriptExportField>();

		public sealed override string Keyword => throw new NotSupportedException();
		public sealed override bool IsStruct => throw new NotSupportedException();
		public sealed override bool IsSerializable => throw new NotSupportedException();
	}
}