using System;
using System.Collections.Generic;

namespace UtinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportArray : ScriptExportType
	{      
		public override void GetUsedNamespaces(ICollection<string> namespaces)
		{
			Element.GetTypeNamespaces(namespaces);
		}

		public override IReadOnlyList<ScriptExportType> GenericArguments { get; } = new ScriptExportType[0];
		public override IReadOnlyList<ScriptExportType> NestedTypes { get; } = new ScriptExportType[0];
		public override IReadOnlyList<ScriptExportEnum> NestedEnums { get; } = new ScriptExportEnum[0];
		public override IReadOnlyList<ScriptExportDelegate> Delegates { get; } = new ScriptExportDelegate[0];
		public override IReadOnlyList<ScriptExportField> Fields { get; } = new ScriptExportField[0];

		protected override ScriptExportType Base => throw new NotSupportedException();
		protected override string Keyword => throw new NotSupportedException();
		protected abstract ScriptExportType Element { get; }

		protected override bool IsStruct => throw new NotSupportedException();
		protected override bool IsSerializable => throw new NotSupportedException();
	}
}