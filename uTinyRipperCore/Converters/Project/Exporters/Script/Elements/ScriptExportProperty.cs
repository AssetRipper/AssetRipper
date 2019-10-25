using System.Collections.Generic;
using System.IO;

namespace uTinyRipper.Converters.Script
{
	public abstract class ScriptExportProperty
	{
		public abstract void Init(IScriptExportManager manager);

		public void Export(TextWriter writer, int intent)
		{
			writer.WriteIndent(intent);
			string sharedKeyword = PropertyKeyword;
			writer.WriteLine("{0} override {1} {2}", sharedKeyword, Type.GetTypeNestedName(DeclaringType), Name);
			writer.WriteIndent(intent);
			writer.WriteLine("{");

			if (HasGetter)
			{
				writer.WriteIndent(intent + 1);
				if (GetKeyword != sharedKeyword)
				{
					writer.WriteLine("{0} ", GetKeyword);
				}
				writer.WriteLine("get {{ return default({0}); }}", Type.NestedName);
			}
			if (HasSetter)
			{
				writer.WriteIndent(intent + 1);
				if (SetKeyword != sharedKeyword)
				{
					writer.WriteLine("{0} ", SetKeyword);
				}
				writer.WriteLine("set {}");
			}

			writer.WriteIndent(intent);
			writer.WriteLine("}");
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

		public abstract ScriptExportType DeclaringType { get; }
		public abstract ScriptExportType Type { get; }

		protected abstract bool HasGetter { get; }
		protected abstract bool HasSetter { get; }
		protected abstract string GetKeyword { get; }
		protected abstract string SetKeyword { get; }

		private string PropertyKeyword
		{
			get
			{
				if (HasGetter && !HasSetter)
				{
					return GetKeyword;
				}
				if (HasSetter && !HasGetter)
				{
					return SetKeyword;
				}
				if (GetKeyword == PublicKeyWord || SetKeyword == PublicKeyWord)
				{
					return PublicKeyWord;
				}
				if (GetKeyword == InternalKeyWord || SetKeyword == InternalKeyWord)
				{
					return InternalKeyWord;
				}
				if (GetKeyword == ProtectedKeyWord || SetKeyword == ProtectedKeyWord)
				{
					return ProtectedKeyWord;
				}
				return PrivateKeyWord;
			}
		}

		protected const string PublicKeyWord = "public";
		protected const string InternalKeyWord = "internal";
		protected const string ProtectedKeyWord = "protected";
		protected const string PrivateKeyWord = "private";
	}
}
