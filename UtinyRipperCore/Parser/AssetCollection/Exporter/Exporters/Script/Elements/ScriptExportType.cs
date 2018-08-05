using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportType
	{
		public abstract void Init(IScriptExportManager manager);

		public abstract ScriptExportType GetContainer(IScriptExportManager manager);

		public ScriptExportType GetTopmostContainer(IScriptExportManager manager)
		{
			ScriptExportType current = GetContainer(manager);
			while(true)
			{
				ScriptExportType next = current.GetContainer(manager);
				if(current == next)
				{
					break;
				}
				current = next;
			}
			return current;
		}

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

		public void GetTypeNamespaces(ICollection<string> namespaces)
		{
			namespaces.Add(Namespace);
			foreach (ScriptExportType arg in GenericArguments)
			{
				arg.GetTypeNamespaces(namespaces);
			}
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
		
		public abstract IReadOnlyList<ScriptExportType> GenericArguments { get; }
		public abstract IReadOnlyList<ScriptExportType> NestedTypes { get; }
		public abstract IReadOnlyList<ScriptExportEnum> NestedEnums { get; }
		public abstract IReadOnlyList<ScriptExportDelegate> Delegates { get; }
		public abstract IReadOnlyList<ScriptExportField> Fields { get; }

		protected abstract ScriptExportType Base { get; }

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
	}
}
