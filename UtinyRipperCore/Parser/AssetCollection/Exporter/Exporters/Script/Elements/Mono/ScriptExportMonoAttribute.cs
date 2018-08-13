using Mono.Cecil;
using System;
using System.IO;

namespace UtinyRipper.Exporters.Scripts.Mono
{
	public sealed class ScriptExportMonoAttribute : ScriptExportAttribute
	{
		public ScriptExportMonoAttribute(CustomAttribute attribute)
		{
			if (attribute == null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}
			
			Attribute = attribute;

			m_module = ScriptExportMonoType.GetModule(Attribute.AttributeType);
			m_fullName = ScriptExportManager.ToFullName(Module, Attribute.AttributeType.FullName);
		}

		public static bool IsCompilerGenerated(TypeDefinition type)
		{
			foreach(CustomAttribute attr in type.CustomAttributes)
			{
				if(attr.AttributeType.Name == CompilerGeneratedName && attr.AttributeType.Namespace == CompilerServicesNamespace)
				{
					return true;
				}
			}
			return false;
		}

		public static string ToFullName(CustomAttribute attribute)
		{
			return ScriptExportManager.ToFullName(GetModule(attribute.AttributeType), attribute.AttributeType.FullName);
		}

		private static string GetModule(TypeReference type)
		{
			return Path.GetFileNameWithoutExtension(type.Scope.Name);
		}

		public override void Init(IScriptExportManager manager)
		{
			m_type = manager.RetrieveType(Attribute.AttributeType);
		}

		public static bool IsSerializableAttribute(CustomAttribute attr)
		{
			TypeReference attrType = attr.AttributeType;
			return attrType.Namespace == SystemNamespace && attrType.Name == SerializableName;
		}

		public static bool IsSerializeFieldAttribute(CustomAttribute attr)
		{
			TypeReference attrType = attr.AttributeType;
			return attrType.Namespace == UnityEngineNamespace && attrType.Name == SerializeFieldName;
		}

		public override string FullName => m_fullName;
		public override string Name => Attribute.AttributeType.Name;
		public override string Module => m_module;

		protected override ScriptExportType Type => m_type;

		private CustomAttribute Attribute { get; }

		private ScriptExportType m_type;
		private string m_fullName;
		private string m_module;
	}
}
