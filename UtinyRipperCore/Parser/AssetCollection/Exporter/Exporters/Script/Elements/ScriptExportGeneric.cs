using System;
using System.Collections.Generic;

namespace UtinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportGeneric : ScriptExportType
	{
		public override void GetTypeNamespaces(ICollection<string> namespaces)
		{
			base.GetTypeNamespaces(namespaces);
			foreach (ScriptExportType argument in Arguments)
			{
				argument.GetTypeNamespaces(namespaces);
			}
		}

		public override void GetUsedNamespaces(ICollection<string> namespaces)
		{
			foreach (ScriptExportType argument in Arguments)
			{
				argument.GetTypeNamespaces(namespaces);
			}
			Owner.GetUsedNamespaces(namespaces);
		}

		public sealed override ScriptExportType DeclaringType => Owner.DeclaringType;
		public abstract ScriptExportType Owner { get; } 
		public abstract IReadOnlyList<ScriptExportType> Arguments { get; }

		public sealed override IReadOnlyList<ScriptExportField> Fields { get; } = new ScriptExportField[0];

		protected sealed override string Keyword => throw new NotSupportedException();

		protected sealed override bool IsStruct => throw new NotSupportedException();
		protected sealed override bool IsSerializable => throw new NotSupportedException();
	}
}
