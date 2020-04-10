using System;
using System.Collections.Generic;
using System.IO;

namespace uTinyRipper.Converters.Script
{
	public abstract class ScriptExportParameter
	{
		public abstract void Init(IScriptExportManager manager);

		public void Export(TextWriter writer, int intent)
		{
			if (IsByRef == ByRefType.None) { }
			else if (IsByRef == ByRefType.Out) writer.Write("out ");
			else if (IsByRef == ByRefType.In && HasInParameters) writer.Write("in ");
			else writer.Write("ref ");
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

		public abstract ScriptExportType Type { get; }

		public abstract string Name { get; }

		public abstract ByRefType IsByRef { get; }

		private bool HasInParameters => false;

		[Flags]
		public enum ByRefType
		{
			None = 0,
			In = 1,
			Out = 2,
			InOut = In | Out,
			Ref = InOut
		}
	}
}
