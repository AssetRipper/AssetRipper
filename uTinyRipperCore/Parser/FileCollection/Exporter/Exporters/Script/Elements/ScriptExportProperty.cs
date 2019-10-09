using System.Collections.Generic;
using System.IO;

namespace uTinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportProperty
	{
		public abstract void Init(IScriptExportManager manager);

		public void Export(TextWriter writer, int intent)
		{
			writer.WriteIndent(intent);
			if (IsOverride) writer.Write("{0} ", "override");
			bool sharedKeyword = false;
			if (HasGetter && !HasSetter) sharedKeyword = true;
			else if(!HasGetter && HasSetter) sharedKeyword = true;
			else if(GetKeyword == SetKeyword) sharedKeyword = true;
			if (sharedKeyword) writer.Write("{0} ", HasGetter ? GetKeyword : SetKeyword);
			writer.WriteLine("{0} {1} {{", PropertyType.GetTypeQualifiedName(DeclaringType), Name);
			if (HasGetter)
			{
				writer.WriteIndent(intent + 1);
				if(!sharedKeyword) writer.WriteLine("{0} ", GetKeyword);
				writer.WriteLine("get {{ return typeof({0}).IsValueType ? ({0})System.Activator.CreateInstance(typeof({0})) : ({0})(object)null; }}", PropertyType.NestedName);
			}
			if (HasSetter)
			{
				writer.WriteIndent(intent + 1);
				if (!sharedKeyword) writer.WriteLine("{0} ", SetKeyword);
				writer.WriteLine("set {  }");
			}
			writer.WriteIndent(intent);
			writer.WriteLine("}");
		}

		public void GetUsedNamespaces(ICollection<string> namespaces)
		{
			PropertyType.GetTypeNamespaces(namespaces);
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
		protected abstract bool HasGetter { get; }
		protected abstract bool HasSetter { get; }
		public abstract ScriptExportType PropertyType { get; }
		public abstract ScriptExportType DeclaringType { get; }

		public abstract string Name { get; }

		protected abstract string GetKeyword { get; }
		protected abstract string SetKeyword { get; }

		protected const string PublicKeyWord = "public";
		protected const string InternalKeyWord = "internal";
		protected const string ProtectedKeyWord = "protected";
		protected const string PrivateKeyWord = "private";
	}
}
