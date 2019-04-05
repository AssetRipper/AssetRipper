using System;
using System.Collections.Generic;
using System.IO;

namespace uTinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportDelegate : ScriptExportType
	{
		public sealed override void Export(TextWriter writer, int intent)
		{
			writer.WriteIndent(intent);
			writer.Write("{0} delegate {1} {2}(", Keyword, Return.NestedName, TypeName);
			for (int i = 0; i < Parameters.Count; i++)
			{
				ScriptExportParameter parameter = Parameters[i];
				parameter.Export(writer, intent);
				if(i < Parameters.Count - 1)
				{
					writer.Write(',');
				}
			}
			writer.WriteLine(");");
		}

		public sealed override void GetUsedNamespaces(ICollection<string> namespaces)
		{
			GetTypeNamespaces(namespaces);
			Return.GetTypeNamespaces(namespaces);
			foreach (ScriptExportParameter parameter in Parameters)
			{
				parameter.GetUsedNamespaces(namespaces);
			}
		}

		public sealed override bool HasMember(string name)
		{
			throw new NotSupportedException();
		}

		public sealed override string CleanNestedName => NestedName;

		public sealed override ScriptExportType Base => null;
		public sealed override IReadOnlyList<ScriptExportField> Fields { get; } = new ScriptExportField[0];
		public abstract ScriptExportType Return { get; }
		public abstract IReadOnlyList<ScriptExportParameter> Parameters { get; }

		protected sealed override bool IsStruct => false;
		protected sealed override bool IsSerializable => false;

		protected const string SystemName = "System";

		protected const string MulticastDelegateName = "MulticastDelegate";
		protected const string InvokeMethodName = "Invoke";
	}
}
