using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper;

namespace UtinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportAttribute
	{
		public abstract void Init(IScriptExportManager manager);

		public void Export(TextWriter writer, int intent)
		{
			writer.WriteIntent(intent);
			writer.WriteLine("[{0}]", Name);
		}

		public void GetUsedNamespaces(ICollection<string> namespaces)
		{
			Type.GetTypeNamespaces(namespaces);
		}

		public abstract string Name { get; }
		public abstract string FullName { get; }
		public abstract string Module { get; }

		protected abstract ScriptExportType Type { get; }
		
		public const string SystemNamespace = "System";
		protected const string UnityEngineNamespace = "UnityEngine";
		protected const string CompilerServicesNamespace = "System.Runtime.CompilerServices";

		public const string SerializableName = "Serializable";
		protected const string SerializeFieldName = "SerializeField";
		protected const string CompilerGeneratedName = "CompilerGeneratedAttribute";
	}
}
