using System.Collections.Generic;
using System.IO;
using System.Linq;
using uTinyRipper.Assembly;

namespace uTinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportType
	{
		public virtual void Init(IScriptExportManager manager)
		{
			m_manager = manager;
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
				writer.WriteIndent(intent);
				writer.WriteLine("[{0}]", ScriptExportAttribute.SerializableName);
			}

			writer.WriteIndent(intent);
			writer.Write("{0} {1} {2}", Keyword, IsStruct ? "struct" : "class", TypeName);
			

			if (Base != null && !SerializableType.IsBasic(Base.Namespace, Base.NestedName))
			{
				writer.Write(" : {0}", Base.GetTypeQualifiedName(this));
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

			foreach (ScriptExportMethod method in Methods)
			{
				method.Export(writer, intent);
			}
			foreach (ScriptExportProperty property in Properties)
			{
				property.Export(writer, intent);
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
			foreach (ScriptExportDelegate @delegate in Delegates)
			{
				@delegate.GetUsedNamespaces(namespaces);
			}
			foreach (ScriptExportField field in Fields)
			{
				field.GetUsedNamespaces(namespaces);
			}
			foreach (ScriptExportMethod method in Methods)
			{
				method.GetUsedNamespaces(namespaces);
			}
			foreach (ScriptExportProperty property in Properties)
			{
				property.GetUsedNamespaces(namespaces);
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

		public virtual string GetTypeQualifiedName(ScriptExportType relativeType)
		{
			if (SerializableType.IsEngineObject(Namespace, NestedName))
			{
				return $"{Namespace}.{NestedName}";
			}
			string typeName = relativeType == null ? NestedName : TypeName;
			if (NestType == null && IsAmbiguous(relativeType))
			{
				typeName = string.IsNullOrEmpty(Namespace) ? typeName : $"{Namespace}.{typeName}";
			}
			if (relativeType == null)
			{
				return typeName;
			}
			if (DeclaringType == null)
			{
				return typeName;
			}
			if (relativeType == DeclaringType)
			{
				return typeName;
			}
			string declaringName = NestType.GetTypeQualifiedName(relativeType);
			return $"{declaringName}.{typeName}";
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
		private HashSet<string> GetAmbiguous(ScriptExportType relativeType)
		{
			// TODO: Check if a typename conflicts with a Unity or System type that is not directly referenced
			// Affected Games: BattleTech
			HashSet<string> usedNamespaces = new HashSet<string>();
			relativeType.GetUsedNamespaces(usedNamespaces);
			foreach (var ns in usedNamespaces.ToArray())
			{
				var parts = ns.Split('.');
				for (int i = 1; i < parts.Length; i++)
				{
					usedNamespaces.Add(string.Join(".", parts.Take(i)));
				}
			}
			ICollection<string> allNamespaces = m_manager.Namespaces;
			ICollection<string> typeNames = m_manager.TypeNames;
			string nestedName = GetTypeNestedName(relativeType);
			string baseName = nestedName.Split('.').First();
			HashSet<string> found = new HashSet<string>();
			//Check if caller containing type has same name 
			var parent = relativeType;
			while (parent != null)
			{
				if (TypeName == parent.TypeName)
				{
					found.Add($"T:{parent.Namespace}.{parent.CleanNestedName}");
				}
				if (parent.Base != null && TypeName == parent.Base.TypeName)
				{
					found.Add($"T:{parent.Base.Namespace}.{parent.Base.CleanNestedName}");
				}
				parent = parent.DeclaringType;
			}
			//Check if typename matches namespace of used namespace 
			foreach (string ns in usedNamespaces)
			{
				string fullName = $"{ns}.{baseName}";
				if (allNamespaces.Contains(fullName))
				{
					found.Add($"NS:{fullName}");
				}
			}
			//Check if typename matches any imported types 
			foreach (string ns in usedNamespaces)
			{
				string fullName = $"{ns}.{nestedName}";
				if(typeNames.Contains(fullName))
				{
					found.Add($"T:{fullName}");
				}
			}
			return found;
		}
		HashSet<string> GetAmbiguous(ScriptExportType relativeType, bool isGenericArgument)
		{
			//If a field uses a type in the same scope that the type is defined in
			if (this is ScriptExportArray)
			{
				return (this as ScriptExportArray).Element.GetAmbiguous(relativeType, isGenericArgument);
			}
			if (NestType != null && !isGenericArgument)
			{
				return NestType.GetAmbiguous(relativeType, isGenericArgument);
			}
			var found = GetAmbiguous(relativeType);
			return found;
		}
		internal bool IsAmbiguous(ScriptExportType relativeType)
		{
			if (IsPrimative) return false;
			return GetAmbiguous(relativeType, false).Count > 1;
		}
		internal bool IsAmbiguousArgument(ScriptExportType callingType)
		{
			if (IsPrimative) return false;
			return GetAmbiguous(callingType, true).Count > 1;
		}
#if DEBUG
		public void LogAmbiguous(ScriptExportType callingType, TextWriter writer, int intent)
		{
			writer.WriteIndent(intent);
			string text = "";
			if (this is ScriptExportGeneric generic)
			{
				text = string.Join(", ", generic.Template.GetAmbiguous(callingType, false));
				text += "<";
				foreach(var arg in generic.Arguments)
				{
					text += "[";
					text += string.Join(", ", arg.GetAmbiguous(callingType, false));
					text += "],";
				}
				text += ">";
			}
			else
			{
				text = string.Join(", ", GetAmbiguous(callingType, false));
			}
			writer.WriteLine($"//Ambiguous: {text}");
		}
#endif
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
			if(namespaces.Count > 0)
			{
				writer.WriteLine();
			}
		}

		private string ExportNamespace()
		{
			if(Namespace == SerializableType.UnityEngineNamespace)
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

		public abstract string FullName { get; }
		public abstract string NestedName { get; }
		public abstract string CleanNestedName { get; }
		public abstract string TypeName { get; }
		public abstract string Namespace { get; }
		public abstract string Module { get; }
		public virtual bool IsEnum => false;
		public abstract bool IsPrimative { get; }

		/// <summary>
		/// ex. GenericClass<T>.NestedType
		/// </summary>
		public abstract ScriptExportType DeclaringType { get; }
		/// <summary>
		/// ex. GenericClass<double>.NestedType
		/// </summary>
		public virtual ScriptExportType NestType => DeclaringType;
		public abstract ScriptExportType Base { get; }

		public IReadOnlyList<ScriptExportType> NestedTypes => m_nestedTypes;
		public IReadOnlyList<ScriptExportEnum> NestedEnums => m_nestedEnums;
		public IReadOnlyList<ScriptExportDelegate> Delegates => m_nestedDelegates;
		public abstract IReadOnlyList<ScriptExportField> Fields { get; }
		public abstract IReadOnlyList<ScriptExportMethod> Methods { get; }
		public abstract IReadOnlyList<ScriptExportProperty> Properties { get; }

		protected abstract string Keyword { get; }
		protected abstract bool IsStruct { get; }
		protected abstract bool IsSerializable { get; }

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
