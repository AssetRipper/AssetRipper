using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UtinyRipper.Exporters.Scripts.Mono;

namespace UtinyRipper.Exporters.Scripts
{
	public sealed class ScriptExportManager : IScriptExportManager
	{
		public ScriptExportManager(string exportPath)
		{
			if(string.IsNullOrEmpty(exportPath))
			{
				throw new ArgumentNullException(nameof(exportPath));
			}
			m_exportPath = exportPath;
		}

		private static string GetExportSubPath(string assembly, string @namespace, string @class)
		{
			string assFolderName = Path.GetFileNameWithoutExtension(assembly);
			string nsFolder = @namespace.Replace('.', Path.DirectorySeparatorChar);
			string finalFolderPath = Path.Combine(assFolderName, nsFolder);
			string filePath = Path.Combine(finalFolderPath, @class);
			return $"{filePath}.cs";
		}

		private static string GetExportSubPath(ScriptExportType type)
		{
			string typeName = type.Name;
			string fixedTypeName = m_illegal.Replace(typeName, "_");
			return GetExportSubPath(type.Module, type.Namespace, fixedTypeName);
		}

		public ScriptExportType CreateExportType(TypeReference type)
		{
			ScriptExportType exportType = RetrieveType(type);
			return exportType;
		}

		public string Export(ScriptExportType exportType)
		{
#if DEBUG
			ScriptExportType topType = exportType.GetTopmostContainer(this);
			if (topType != exportType)
			{
				throw new NotSupportedException("You can export only topmost types");
			}
#endif

			if (IsBuiltInType(exportType))
			{
				return null;
			}

			string subPath = GetExportSubPath(exportType);
			string filePath = Path.Combine(m_exportPath, subPath);
			string directory = Path.GetDirectoryName(filePath);
			if(!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			using (FileStream file = File.OpenWrite(filePath))
			{
				using (StreamWriter writer = new StreamWriter(file))
				{
					exportType.Export(writer);
				}
			}
			AddExportedType(exportType);
			return filePath;
		}

		public void ExportRest()
		{
			foreach (ScriptExportType type in m_types.Values)
			{
				ScriptExportType top = type.GetTopmostContainer(this);
				if (m_exported.Contains(top.FullName))
				{
					continue;
				}

				Export(top);
			}

			foreach (ScriptExportEnum @enum in m_enums.Values)
			{
				ScriptExportType top = @enum.GetTopmostContainer(this);
				if(top != @enum)
				{
					if (m_exported.Contains(top.FullName))
					{
						continue;
					}
				}

				Export(top);
			}

			foreach (ScriptExportDelegate @delegate in m_delegates.Values)
			{
				ScriptExportType top = @delegate.GetTopmostContainer(this);
				if (top != @delegate)
				{
					if (m_exported.Contains(top.FullName))
					{
						continue;
					}
				}

				Export(top);
			}
		}
		
		public ScriptExportType RetrieveType(TypeReference type)
		{
			if(type.Module != null)
			{
				TypeDefinition definition = type.Resolve();
				if(definition != null)
				{
					if (definition.IsEnum)
					{
						return RetrieveEnum(definition);
					}
					if (ScriptExportMonoDelegate.IsDelegate(definition))
					{
						return RetrieveDelegate(definition);
					}
				}
			}

			if (m_types.TryGetValue(type.FullName, out ScriptExportType exportType))
			{
				return exportType;
			}
			return CreateType(type);
		}

		public ScriptExportEnum RetrieveEnum(TypeDefinition @enum)
		{
			if (m_enums.TryGetValue(@enum.FullName, out ScriptExportEnum exportEnum))
			{
				return exportEnum;
			}
			return CreateEnum(@enum);
		}

		public ScriptExportDelegate RetrieveDelegate(TypeDefinition @delegate)
		{
			if (m_delegates.TryGetValue(@delegate.FullName, out ScriptExportDelegate exportDelegate))
			{
				return exportDelegate;
			}
			return CreateDelegate(@delegate);
		}

		public ScriptExportAttribute RetrieveAttribute(CustomAttribute attribute)
		{
			if (m_attributes.TryGetValue(attribute.AttributeType.FullName, out ScriptExportAttribute exportAttribute))
			{
				return exportAttribute;
			}
			return CreateAttribute(attribute);
		}

		public ScriptExportField RetrieveField(FieldDefinition field)
		{
			ScriptExportField exportField = new ScriptExportMonoField(field);
			exportField.Init(this);
			return exportField;
		}

		public ScriptExportParameter RetrieveParameter(ParameterDefinition parameter)
		{
			ScriptExportParameter exportParameter = new ScriptExportMonoParameter(parameter);
			exportParameter.Init(this);
			return exportParameter;
		}

		private ScriptExportType CreateType(TypeReference type)
		{
			ScriptExportType exportType = new ScriptExportMonoType(type);
			m_types.Add(type.FullName, exportType);
			exportType.Init(this);
			return exportType;
		}

		private ScriptExportEnum CreateEnum(TypeReference @enum)
		{
			ScriptExportMonoEnum exportEnum = new ScriptExportMonoEnum(@enum);
			m_enums.Add(@enum.FullName, exportEnum);
			exportEnum.Init(this);
			return exportEnum;
		}

		private ScriptExportDelegate CreateDelegate(TypeDefinition @delegate)
		{
			ScriptExportMonoDelegate exportDelegate = new ScriptExportMonoDelegate(@delegate);
			m_delegates.Add(@delegate.FullName, exportDelegate);
			exportDelegate.Init(this);
			return exportDelegate;
		}

		private ScriptExportAttribute CreateAttribute(CustomAttribute attribute)
		{
			ScriptExportMonoAttribute exportAttribute = new ScriptExportMonoAttribute(attribute);
			m_attributes.Add(attribute.AttributeType.FullName, exportAttribute);
			exportAttribute.Init(this);
			return exportAttribute;
		}

		private void AddExportedType(ScriptExportType exportType)
		{
			m_exported.Add(exportType.FullName);
			foreach (ScriptExportEnum nestedEnum in exportType.NestedEnums)
			{
				m_exported.Add(nestedEnum.FullName);
			}
			foreach (ScriptExportDelegate @delegate in exportType.Delegates)
			{
				m_exported.Add(@delegate.FullName);
			}

			foreach (ScriptExportType nestedType in exportType.NestedTypes)
			{
				AddExportedType(nestedType);
			}
		}

		private static bool IsBuiltInType(ScriptExportType type)
		{
			if (IsDotNetLibrary(type.Module))
			{
				return true;
			}
			if (IsUnityLibrary(type.Module))
			{
				//return true;
			}

			return false;
		}

		private static bool IsDotNetLibrary(string module)
		{
			switch(module)
			{
				case MSCoreLibName:
				case SystemName:
					return true;

				default:
					return module.StartsWith($"{SystemName}.", StringComparison.Ordinal);
			}
		}

		private static bool IsUnityLibrary(string module)
		{
			switch (module)
			{
				case UnityEngineName:
				case BooName:
					return true;

				default:
					{
						if (module.StartsWith($"{UnityEngineName}.", StringComparison.Ordinal))
						{
							return true;
						}
						if (module.StartsWith($"{MonoName}.", StringComparison.Ordinal))
						{
							return true;
						}
						return false;
					}
			}
		}

		private const string MSCoreLibName = "mscorlib";
		private const string SystemName = "System";
		private const string UnityEngineName = "UnityEngine";
		private const string BooName = "Boo";
		private const string MonoName = "Mono";

		private static readonly Regex m_illegal = new Regex("[<>]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private readonly Dictionary<string, ScriptExportType> m_types = new Dictionary<string, ScriptExportType>();
		private readonly Dictionary<string, ScriptExportEnum> m_enums = new Dictionary<string, ScriptExportEnum>();
		private readonly Dictionary<string, ScriptExportDelegate> m_delegates = new Dictionary<string, ScriptExportDelegate>();
		private readonly Dictionary<string, ScriptExportAttribute> m_attributes = new Dictionary<string, ScriptExportAttribute>();

		private readonly HashSet<string> m_exported = new HashSet<string>();

		private readonly string m_exportPath;
	}
}
