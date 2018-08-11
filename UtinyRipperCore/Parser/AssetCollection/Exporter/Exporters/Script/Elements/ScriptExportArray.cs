using System;
using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportArray : ScriptExportType
	{      
		public override void GetUsedNamespaces(ICollection<string> namespaces)
		{
			GetTypeNamespaces(namespaces);
			Element.GetTypeNamespaces(namespaces);
		}

		protected override ScriptExportType Base => null;

		public override IReadOnlyList<ScriptExportType> GenericArguments { get; } = new ScriptExportType[0];
		public override IReadOnlyList<ScriptExportType> NestedTypes { get; } = new ScriptExportType[0];
		public override IReadOnlyList<ScriptExportEnum> NestedEnums { get; } = new ScriptExportEnum[0];
		public override IReadOnlyList<ScriptExportDelegate> Delegates { get; } = new ScriptExportDelegate[0];
		public override IReadOnlyList<ScriptExportField> Fields { get; } = new ScriptExportField[0];

		protected abstract ScriptExportType Element { get; }

		protected override bool IsStruct => false;
		protected override bool IsSerializable => false;

	}
}