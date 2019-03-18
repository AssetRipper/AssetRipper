using System;
using System.Collections.Generic;

namespace uTinyRipper.Exporters.Scripts
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

		public sealed override IReadOnlyList<ScriptExportField> Fields { get; } = new ScriptExportField[0];

		protected sealed override string Keyword => throw new NotSupportedException();

		protected sealed override bool IsStruct => throw new NotSupportedException();
		protected sealed override bool IsSerializable => throw new NotSupportedException();
	}
}
