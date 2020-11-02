using System.Collections.Generic;
using System.IO;
using uTinyRipper.Game.Assembly;

namespace uTinyRipper.Converters.Script
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
				writer.WriteIndent(intent);
				writer.WriteLine("[{0}]", ScriptExportAttribute.SerializableName);
			}

			writer.WriteIndent(intent);
			writer.Write("{0} {1} {2}", Keyword, IsStruct ? "struct" : "class", TypeName);
			if (Base != null && !SerializableType.IsBasic(Base.Namespace, Base.NestedName))
			{
				writer.Write(" : {0}", Base.GetTypeNestedName(DeclaringType));
			}
			writer.WriteLine();
			writer.WriteIndent(intent++);
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

			foreach (ScriptExportDelegate @delegate in NestedDelegates)
			{
				@delegate.Export(writer, intent);
			}
			if (NestedDelegates.Count > 0)
			{
				writer.WriteLine();
			}

			if (Constructor != null)
			{
				Constructor.Export(writer, intent);
				writer.WriteLine();
			}
			foreach (ScriptExportMethod method in Methods)
			{
				method.Export(writer, intent);
				writer.WriteLine();
			}
			foreach (ScriptExportProperty property in Properties)
			{
				property.Export(writer, intent);
			}
			if (Properties.Count > 0)
			{
				writer.WriteLine();
			}
			foreach (ScriptExportField field in Fields)
			{
				field.Export(writer, intent);
			}

			writer.WriteIndent(--intent);
			writer.WriteLine('}');
		}

		public virtual void GetTypeNamespaces(ICollection<string> namespaces)
		{
			if (SerializableType.IsCPrimitive(Namespace, CleanNestedName))
			{
				return;
			}
			namespaces.Add(ExportNamespace());
		}

		public virtual void GetUsedNamespaces(ICollection<string> namespaces)
		{
			GetTypeNamespaces(namespaces);
			if (Base != null)
			{
				Base.GetTypeNamespaces(namespaces);
			}
			foreach (ScriptExportType nestedType in NestedTypes)
			{
				nestedType.GetUsedNamespaces(namespaces);
			}
			foreach (ScriptExportDelegate @delegate in NestedDelegates)
			{
				@delegate.GetUsedNamespaces(namespaces);
			}
			Constructor?.GetUsedNamespaces(namespaces);
			foreach (ScriptExportMethod method in Methods)
			{
				method.GetUsedNamespaces(namespaces);
			}
			foreach (ScriptExportProperty property in Properties)
			{
				property.GetUsedNamespaces(namespaces);
			}
			foreach (ScriptExportField field in Fields)
			{
				field.GetUsedNamespaces(namespaces);
			}
		}

		public virtual bool HasMember(string name)
		{
			switch (name)
			{
				case "audio":
				case "light":
				case "collider":
					return SerializableType.IsComponent(Namespace, NestedName);
			}
			return false;
		}

		public string GetTypeNestedName(ScriptExportType relativeType)
		{
			string typeName = TypeName;
			if (DeclaringType == null)
			{
				return TypeName;
			}
			if (relativeType == DeclaringType)
			{
				return TypeName;
			}
			string declaringName = NestType.GetTypeNestedName(relativeType);
			return $"{declaringName}.{typeName}";
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

		protected void AddAsNestedType()
		{
			DeclaringType.m_nestedTypes.Add(this);
		}

		protected void AddAsNestedEnum()
		{
			DeclaringType.m_nestedEnums.Add((ScriptExportEnum)this);
		}

		protected void AddAsNestedDelegate()
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
			if (namespaces.Count > 0)
			{
				writer.WriteLine();
			}
		}

		private string ExportNamespace()
		{
			if (Namespace == SerializableType.UnityEngineNamespace)
			{
				switch (CleanNestedName)
				{
					case "NavMeshAgent":
					case "OffMeshLink":
						return $"{SerializableType.UnityEngineNamespace}.AI";
				}
			}
			return Namespace;
		}

		/// <summary>
		/// Full name is a unique name with module prefix
		/// ex: [Module]Namespace.DeclaringClass<DT1, DT2>.Class<T1, T2>
		/// </summary>
		public abstract string FullName { get; }
		/// <summary>
		/// Nested name is type name with declaring type prefix
		/// ex: DeclaringClass<DT1, DT2>.Class<T1, T2>
		/// </summary>
		public abstract string NestedName { get; }
		/// <summary>
		/// Type name with declaring type prefix, but this prefix is cleaned from generic parameters
		/// ex: DeclaringClass.Class<T1, T2>
		/// </summary>
		public abstract string CleanNestedName { get; }
		/// <summary>
		/// Type name without any prexifes
		/// ex: Class<T1, T2>
		/// </summary>
		public abstract string TypeName { get; }
		/// <summary>
		/// Type name without any prefixes or generic parameters
		/// ex: Class
		/// </summary>
		public abstract string CleanName { get; }
		public abstract string Namespace { get; }
		public abstract string Module { get; }
		public virtual bool IsEnum => false;

		/// <summary>
		/// Declaring type with generic parameters (if any)
		/// ex: GenericClass<T>.NestedType
		/// </summary>
		public abstract ScriptExportType DeclaringType { get; }
		/// <summary>
		/// Declaring type with generic arguments (if any)
		/// ex: GenericClass<double>.NestedType
		/// </summary>
		public virtual ScriptExportType NestType => DeclaringType;
		public abstract ScriptExportType Base { get; }

		public IReadOnlyList<ScriptExportType> NestedTypes => m_nestedTypes;
		public IReadOnlyList<ScriptExportEnum> NestedEnums => m_nestedEnums;
		public IReadOnlyList<ScriptExportDelegate> NestedDelegates => m_nestedDelegates;
		public abstract ScriptExportConstructor Constructor { get; }
		public abstract IReadOnlyList<ScriptExportMethod> Methods { get; }
		public abstract IReadOnlyList<ScriptExportProperty> Properties { get; }
		public abstract IReadOnlyList<ScriptExportField> Fields { get; }

		public abstract string Keyword { get; }
		public abstract bool IsStruct { get; }
		public abstract bool IsSerializable { get; }

		protected const string PublicKeyWord = "public";
		protected const string InternalKeyWord = "internal";
		protected const string ProtectedKeyWord = "protected";
		protected const string PrivateKeyWord = "private";

		protected readonly List<ScriptExportType> m_nestedTypes = new List<ScriptExportType>();
		protected readonly List<ScriptExportEnum> m_nestedEnums = new List<ScriptExportEnum>();
		protected readonly List<ScriptExportDelegate> m_nestedDelegates = new List<ScriptExportDelegate>();
		protected IScriptExportManager m_manager;
	}
}
