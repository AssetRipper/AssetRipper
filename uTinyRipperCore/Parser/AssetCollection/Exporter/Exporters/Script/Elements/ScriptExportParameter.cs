using System.Collections.Generic;
using System.IO;

namespace uTinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportParameter
	{
		public abstract void Init(IScriptExportManager manager);

		public void Export(TextWriter writer, int intent)
		{
			writer.Write("{0} {1}", Type.NestedName, Name);
		}

		public void GetUsedNamespaces(ICollection<string> namespaces)
		{
			Type.GetTypeNamespaces(namespaces);
		}

		public override string ToString()
		{
			if (Name == null)
			{
				return base.ToString();
			}
			else
			{
				return Name;
			}
		}

		protected abstract ScriptExportType Type { get; }

		protected abstract string Name { get; }
	}
}
