using System.Collections.Generic;
using System.IO;

namespace uTinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportAttribute
	{
		public abstract void Init(IScriptExportManager manager);

		public void Export(TextWriter writer, int intent)
		{
			writer.WriteIndent(intent);
			writer.WriteLine("[{0}]", Name);
		}

		public void GetUsedNamespaces(ICollection<string> namespaces)
		{
			Type.GetTypeNamespaces(namespaces);
		}

		public override string ToString()
		{
			if (FullName == null)
			{
				return base.ToString();
			}
			else
			{
				return FullName;
			}
		}

		public abstract string Name { get; }
		public abstract string FullName { get; }
		public abstract string Module { get; }

		protected abstract ScriptExportType Type { get; }
		
		public const string SystemNamespace = "System";
		protected const string UnityEngineNamespace = "UnityEngine";

		public const string SerializableName = "Serializable";
		protected const string SerializeFieldName = "SerializeField";
		protected const string MultilineAttributeName = "MultilineAttribute";
		protected const string TextAreaAttributeName = "TextAreaAttribute";
	}
}
