using System;
using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportField
	{
		public abstract void Init(IScriptExportManager manager);

		public void Export(TextWriter writer, int intent)
		{
			if (Attribute != null)
			{
				Attribute.Export(writer, intent);
			}

			writer.WriteIntent(intent);
			writer.Write("{0} ", Keyword);
			if(IsNew)
			{
				writer.Write("new ");
			}
			string name = GetTypeNestedName(Type);
			writer.WriteLine("{0} {1};", name, Name);
		}

		public void ExportEnum(TextWriter writer, int intent)
		{
			if (Type.IsEnum)
			{
				writer.WriteIntent(intent);
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
			if(Attribute != null)
			{
				Attribute.GetUsedNamespaces(namespaces);
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

		private string GetTypeNestedName(ScriptExportType type)
		{
			if(type.DeclaringType == null)
			{
				return type.Name;
			}
			if(type.DeclaringType == DeclaringType)
			{
				return type.Name;
			}

			string declaringName = GetTypeNestedName(type.DeclaringType);
			return $"{declaringName}.{type.Name}";
		}
		
		public abstract string Name { get; }

		public abstract ScriptExportType DeclaringType { get; }
		public abstract ScriptExportType Type { get; }
		public abstract ScriptExportAttribute Attribute { get; }

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
