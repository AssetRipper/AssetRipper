using System;
using System.Collections.Generic;
using System.IO;

namespace uTinyRipper.Converters.Script
{
	public abstract class ScriptExportParameter
	{
		[Flags]
		public enum ByRefType
		{
			None = 0,
			In = 1,
			Out = 2,
			InOut = In | Out,
			Ref = InOut
		}

		public abstract void Init(IScriptExportManager manager);

		public void Export(TextWriter writer, int intent)
		{
			switch (ByRef)
			{
				case ByRefType.Out:
					writer.Write("out ");
					break;
				case ByRefType.In:
					writer.Write("in ");
					break;
				case ByRefType.InOut:
					writer.Write("ref ");
					break;
			}
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

		public abstract string Name { get; }

		public abstract ScriptExportType Type { get; }

		public abstract ByRefType ByRef { get; }
	}
}
