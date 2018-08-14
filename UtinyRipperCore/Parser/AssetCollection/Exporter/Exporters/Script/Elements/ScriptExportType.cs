using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportType
	{
		public abstract void Init(IScriptExportManager manager);
		
		public void Export(TextWriter writer)
		{
			ExportUsings(writer);
			if (Namespace == string.Empty)
			{
				Export(writer, 0);
			}
			else
			{
				writer.WriteLine("namespace {0}", Namespace);
				writer.WriteLine('{');
				Export(writer, 1);
				writer.WriteLine('}');
			}
		}

		public virtual void Export(TextWriter writer, int intent)
		{
			if (IsSerializable)
			{
				writer.WriteIntent(intent);
				writer.WriteLine("[{0}]", ScriptExportAttribute.SerializableName);
			}

			writer.WriteIntent(intent);
			writer.Write("{0} {1} {2}", Keyword, IsStruct ? "struct" : "class", Name);
			if (Base != null && !Base.IsBasic)
			{
				writer.Write(" : {0}", Base.Name);
			}
			writer.WriteLine();

			writer.WriteIntent(intent++);
			writer.WriteLine('{');

			foreach (ScriptExportType nestedType in NestedTypes)
			{
				nestedType.Export(writer, intent);
				writer.WriteLine();
			}

			foreach (ScriptExportEnum nestedEnum in NestedEnums)
			{
				nestedEnum.Export(writer, intent);
				writer.WriteLine();
			}

			foreach (ScriptExportDelegate @delegate in Delegates)
			{
				@delegate.Export(writer, intent);
			}
			if (Delegates.Count > 0)
			{
				writer.WriteLine();
			}

			foreach (ScriptExportField field in Fields)
			{
				field.Export(writer, intent);
			}

			writer.WriteIntent(--intent);
			writer.WriteLine('}');
		}

		public virtual void GetTypeNamespaces(ICollection<string> namespaces)
		{
			namespaces.Add(Namespace);
		}

		public virtual void GetUsedNamespaces(ICollection<string> namespaces)
		{
			GetTypeNamespaces(namespaces);
			if (Base != null)
			{
				namespaces.Add(Base.Namespace);
			}
			foreach(ScriptExportType nestedType in NestedTypes)
			{
				nestedType.GetUsedNamespaces(namespaces);
			}
			foreach (ScriptExportDelegate @delegate in Delegates)
			{
				@delegate.GetUsedNamespaces(namespaces);
			}
			foreach (ScriptExportField field in Fields)
			{
				field.GetUsedNamespaces(namespaces);
			}
		}

		public bool HasMember(string name)
		{
			return HasMember(this, name);
		}

		protected bool HasMember(ScriptExportType type, string name)
		{
			foreach (ScriptExportField field in type.Fields)
			{
				if (field.Name == Name)
				{
					return true;
				}
			}

			if (type.Base == null)
			{
				return HasMemberInner(name);
			}
			else
			{
				return HasMember(type.Base, name);
			}
		}

		protected abstract bool HasMemberInner(string name);

		public override string ToString()
		{
			if(FullName == null)
			{
				return base.ToString();
			}
			else
			{
				return FullName;
			}
		}

		protected void AddAsNestedType(IScriptExportManager manager)
		{
			DeclaringType.m_nestedTypes.Add(this);
		}

		protected void AddAsNestedEnum(IScriptExportManager manager)
		{
			DeclaringType.m_nestedEnums.Add((ScriptExportEnum)this);
		}

		protected void AddAsNestedDelegate(IScriptExportManager manager)
		{
			DeclaringType.m_nestedDelegates.Add((ScriptExportDelegate)this);
		}

		private void ExportUsings(TextWriter writer)
		{
			HashSet<string> namespaces = new HashSet<string>();
			GetUsedNamespaces(namespaces);
			namespaces.Remove(string.Empty);
			namespaces.Remove(Namespace);
			foreach (string @namespace in namespaces)
			{
				writer.WriteLine("using {0};", @namespace);
			}
			if(namespaces.Count > 0)
			{
				writer.WriteLine();
			}
		}

		public abstract string FullName { get; }
		public abstract string Name { get; }
		public abstract string Namespace { get; }
		public abstract string Module { get; }
		public virtual bool IsEnum => false;

		public abstract ScriptExportType DeclaringType { get; }
		public abstract ScriptExportType Base { get; }

		public IReadOnlyList<ScriptExportType> NestedTypes => m_nestedTypes;
		public IReadOnlyList<ScriptExportEnum> NestedEnums => m_nestedEnums;
		public IReadOnlyList<ScriptExportDelegate> Delegates => m_nestedDelegates;
		public abstract IReadOnlyList<ScriptExportField> Fields { get; }

		protected abstract string Keyword { get; }
		protected abstract bool IsStruct { get; }
		protected abstract bool IsSerializable { get; }

		private bool IsBasic => (Name == ObjectType || Name == ValueType) && Namespace == SystemNamespace;

		protected const string PublicKeyWord = "public";
		protected const string InternalKeyWord = "internal";
		protected const string ProtectedKeyWord = "protected";
		protected const string PrivateKeyWord = "private";

		protected const string SystemNamespace = "System";

		protected const string ObjectType = "Object";
		protected const string ValueType = "ValueType";

		protected readonly List<ScriptExportType> m_nestedTypes = new List<ScriptExportType>();
		protected readonly List<ScriptExportEnum> m_nestedEnums = new List<ScriptExportEnum>();
		protected readonly List<ScriptExportDelegate> m_nestedDelegates = new List<ScriptExportDelegate>();
	}
}
