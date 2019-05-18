using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Assembly;

namespace uTinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportField
	{
		public abstract void Init(IScriptExportManager manager);

		public void Export(TextWriter writer, int intent)
		{
			if (Attributes != null)
			{
				foreach (ScriptExportAttribute attribute in Attributes)
				{
					attribute.Export(writer, intent);
				}
			}

			writer.WriteIndent(intent);
			writer.Write("{0} ", Keyword);
			if(IsNew)
			{
				writer.Write("new ");
			}

			string name = Type.GetTypeNestedName(DeclaringType);
			name = SerializableType.IsEngineObject(Type.Namespace, name) ? $"{Type.Namespace}.{name}" : name;
			writer.WriteLine("{0} {1};", name, Name);
		}

		public void ExportEnum(TextWriter writer, int intent)
		{
			if (Type.IsEnum)
			{
				writer.WriteIndent(intent);
				writer.WriteLine("{0} = {1},", Name, Constant);
			}
			else
			{
				throw new NotSupportedException();
			}
		}
		
		public void GetUsedNamespaces(ICollection<string> namespaces)
		{
			Type.GetTypeNamespaces(namespaces);
			if (Attributes != null)
			{
				foreach (ScriptExportAttribute attribute in Attributes)
				{
					attribute.GetUsedNamespaces(namespaces);
				}
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
		public abstract ScriptExportType Type { get; }
		public abstract IReadOnlyList<ScriptExportAttribute> Attributes { get; }

		protected abstract string Keyword { get; }
		protected bool IsNew
		{
			get
			{
				ScriptExportType baseType = DeclaringType.Base;
				if (baseType == null)
				{
					return false;
				}
				return baseType.HasMember(Name);
			}
		}
		protected abstract string Constant { get; }

		protected const string PublicKeyWord = "public";
		protected const string InternalKeyWord = "internal";
		protected const string ProtectedKeyWord = "protected";
		protected const string PrivateKeyWord = "private";
	}
}
