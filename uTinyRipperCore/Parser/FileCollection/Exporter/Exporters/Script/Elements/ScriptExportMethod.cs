using System.Collections.Generic;
using System.IO;

namespace uTinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportMethod
	{

		public abstract void Init(IScriptExportManager manager);

		public void Export(TextWriter writer, int intent)
		{
			writer.WriteIndent(intent);
			writer.Write("{0} ", Keyword);
			if (IsOverride) writer.Write("{0} ", "override");

			string returnName = ReturnType.GetTypeQualifiedName(DeclaringType);
			writer.Write("{0} {1}(", returnName, Name);
			for(int i = 0; i < Parameters.Count; i++)
			{
				ScriptExportParameter parameter = Parameters[i];
				parameter.Export(writer, intent);
				if (i < Parameters.Count - 1) writer.Write(", ");
			}
			writer.Write("){");
			if(ReturnType.TypeName == "void")
			{
				writer.WriteLine(" }");
			} else
			{
				writer.WriteLine();
				writer.WriteIndent(intent + 1);
				writer.WriteLine("return typeof({0}).IsValueType ? ({0})System.Activator.CreateInstance(typeof({0})) : ({0})(object)null;", returnName);
				writer.WriteIndent(intent);
				writer.WriteLine("}");
			}
		}

		public void GetUsedNamespaces(ICollection<string> namespaces)
		{
			ReturnType.GetTypeNamespaces(namespaces);
			foreach(ScriptExportParameter parameter in Parameters)
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

		protected abstract bool IsOverride { get; }
		public abstract IReadOnlyList<ScriptExportParameter> Parameters { get; }
		public abstract ScriptExportType ReturnType { get; }
		public abstract ScriptExportType DeclaringType { get; }

		public abstract string Name { get; }

		protected abstract string Keyword { get; }

		protected const string PublicKeyWord = "public";
		protected const string InternalKeyWord = "internal";
		protected const string ProtectedKeyWord = "protected";
		protected const string PrivateKeyWord = "private";
	}
}
