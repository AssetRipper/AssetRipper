using System;
using System.Collections.Generic;

namespace uTinyRipper.Converters.Script
{
	public abstract class ScriptExportGeneric : ScriptExportType
	{
		public sealed override void GetTypeNamespaces(ICollection<string> namespaces)
		{
			base.GetTypeNamespaces(namespaces);
			foreach (ScriptExportType argument in Arguments)
			{
				argument.GetTypeNamespaces(namespaces);
			}
		}

		public sealed override void GetUsedNamespaces(ICollection<string> namespaces)
		{
			foreach (ScriptExportType argument in Arguments)
			{
				argument.GetTypeNamespaces(namespaces);
			}
			Template.GetUsedNamespaces(namespaces);
		}

		public sealed override string CleanNestedName => Template.CleanNestedName;
		public sealed override bool IsEnum => Template.IsEnum;

		public sealed override ScriptExportType DeclaringType => Template.DeclaringType;
		public abstract ScriptExportType Template { get; }
		public abstract IReadOnlyList<ScriptExportType> Arguments { get; }
		public sealed override ScriptExportType Base => Template.Base;

		public sealed override ScriptExportConstructor Constructor => null;
		public sealed override IReadOnlyList<ScriptExportMethod> Methods { get; } = Array.Empty<ScriptExportMethod>();
		public sealed override IReadOnlyList<ScriptExportProperty> Properties { get; } = Array.Empty<ScriptExportProperty>();
		public sealed override IReadOnlyList<ScriptExportField> Fields { get; } = Array.Empty<ScriptExportField>();

		public sealed override string Keyword => throw new NotSupportedException();
		public sealed override bool IsStruct => throw new NotSupportedException();
		public sealed override bool IsSerializable => false;
	}
}
