using System.Collections.Generic;
using System.IO;

namespace uTinyRipper.Converters.Script
{
	public abstract class ScriptExportMethod
	{
		public abstract void Init(IScriptExportManager manager);

		public virtual void Export(TextWriter writer, int intent)
		{
			writer.WriteIndent(intent);
			string returnTypeName = ReturnType.GetTypeNestedName(DeclaringType);
			writer.Write("{0} override {1} {2}(", Keyword, returnTypeName, Name);
			for (int i = 0; i < Parameters.Count; i++)
			{
				ScriptExportParameter parameter = Parameters[i];
				parameter.Export(writer, intent);
				if (i < Parameters.Count - 1)
				{
					writer.Write(", ");
				}
			}
			writer.WriteLine(")");
			writer.WriteIndent(intent);
			writer.WriteLine("{");
			foreach (ScriptExportParameter parameter in Parameters)
			{
				if (parameter.ByRef == ScriptExportParameter.ByRefType.Out)
				{
					writer.WriteIndent(intent + 1);
					writer.WriteLine("{0} = default({1});", parameter.Name, parameter.Type.GetTypeNestedName(DeclaringType));
				}
			}
			if (ReturnType.TypeName != MonoUtils.CVoidName)
			{
				writer.WriteIndent(intent + 1);
				writer.WriteLine("return default({0});", returnTypeName);
			}
			writer.WriteIndent(intent);
			writer.WriteLine("}");
		}

		public virtual void GetUsedNamespaces(ICollection<string> namespaces)
		{
			ReturnType.GetTypeNamespaces(namespaces);
			foreach (ScriptExportParameter parameter in Parameters)
			{
				parameter.GetUsedNamespaces(namespaces);
			}
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

		public abstract ScriptExportType DeclaringType { get; }
		public abstract ScriptExportType ReturnType { get; }
		public abstract IReadOnlyList<ScriptExportParameter> Parameters { get; }

		public abstract string Keyword { get; }

		protected const string PublicKeyWord = "public";
		protected const string InternalKeyWord = "internal";
		protected const string ProtectedKeyWord = "protected";
		protected const string PrivateKeyWord = "private";
	}
}
