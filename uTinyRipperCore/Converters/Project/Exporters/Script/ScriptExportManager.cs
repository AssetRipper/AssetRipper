using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using uTinyRipper.Game;
using uTinyRipper.Converters.Script.Mono;
using uTinyRipper.Layout;

namespace uTinyRipper.Converters.Script
{
	public sealed class ScriptExportManager : IScriptExportManager
	{
		public ScriptExportManager(AssetLayout layout, string exportPath)
		{
			if (string.IsNullOrEmpty(exportPath))
			{
				throw new ArgumentNullException(nameof(exportPath));
			}
			Layout = layout;
			m_exportPath = exportPath;
		}

		public static string ToFullName(string module, string fullname)
		{
			return $"[{module}]{fullname}";
		}

		public static bool IsBuiltinLibrary(string module)
		{
			if (IsFrameworkLibrary(module))
			{
				return true;
			}
			if (IsUnityLibrary(module))
			{
				return true;
			}

			return false;
		}

		public static bool IsFrameworkLibrary(string module)
		{
			switch (module)
			{
				case MSCoreLibName:
				case NetStandardName:
				case SystemName:
				case CLRName:
					return true;

				default:
					return module.StartsWith($"{SystemName}.", StringComparison.Ordinal);
			}
		}

		public static bool IsUnityLibrary(string module)
		{
			switch (module)
			{
				case UnityEngineName:
				case BooName:
				case BooLangName:
				case UnityScriptName:
				case UnityScriptLangName:
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

		private static string GetExportSubPath(string assembly, string @namespace, string @class)
		{
			string assemblyFolder = AssemblyManager.ToAssemblyName(assembly);
			string namespaceFolder = @namespace.Replace('.', Path.DirectorySeparatorChar);
			string folderPath = Path.Combine(assemblyFolder, namespaceFolder);
			string filePath = Path.Combine(folderPath, @class);
			return $"{DirectoryUtils.FixInvalidPathCharacters(filePath)}.cs";
		}

		private static string GetExportSubPath(ScriptExportType type)
		{
			string typeName = type.NestedName;
			int index = typeName.IndexOf('<');
			if (index >= 0)
			{
				string normalName = typeName.Substring(0, index);
				typeName = normalName + $".{typeName.Count(t => t == ',') + 1}";
			}
			return GetExportSubPath(type.Module, type.Namespace, typeName);
		}

		public string Export(ScriptExportType exportType)
		{
			if (exportType.DeclaringType != null)
			{
				throw new NotSupportedException("You can export only topmost types");
			}

			if (IsBuiltInType(exportType))
			{
				return null;
			}

			string subPath = GetExportSubPath(exportType);
			string filePath = Path.Combine(m_exportPath, subPath);
			string uniqueFilePath = ToUniqueFileName(filePath);
			string directory = Path.GetDirectoryName(uniqueFilePath);
			if (!DirectoryUtils.Exists(directory))
			{
				DirectoryUtils.CreateVirtualDirectory(directory);
			}

			using (Stream fileStream = FileUtils.CreateVirtualFile(uniqueFilePath))
			{
				using (StreamWriter writer = new InvariantStreamWriter(fileStream, new UTF8Encoding(false)))
				{
					exportType.Export(writer);
				}
			}
			AddExportedType(exportType);
			return uniqueFilePath;
		}

		public void ExportRest()
		{
			foreach (ScriptExportType type in m_types.Values)
			{
				if (type.DeclaringType != null)
				{
					continue;
				}
				if (m_exported.Contains(type.FullName))
				{
					continue;
				}
				
				Export(type);
			}

			foreach (ScriptExportEnum @enum in m_enums.Values)
			{
				if (@enum.DeclaringType != null)
				{
					continue;
				}
				if (m_exported.Contains(@enum.FullName))
				{
					continue;
				}

				Export(@enum);
			}

			foreach (ScriptExportDelegate @delegate in m_delegates.Values)
			{
				if (@delegate.DeclaringType != null)
				{
					continue;
				}
				if (m_exported.Contains(@delegate.FullName))
				{
					continue;
				}

				Export(@delegate);
			}
		}

		public ScriptExportType RetrieveType(TypeReference type)
		{
			if (type.IsArray)
			{
				return RetrieveArray(type);
			}
			if (type.IsPointer)
			{
				return RetrievePointer(type);
			}
			if (type.IsGenericInstance)
			{
				return RetrieveGeneric(type);
			}
			if (type.IsGenericParameter)
			{
				return CreateType(type, false);
			}

			if (type.Module != null)
			{
				TypeDefinition definition = type.Resolve();
				if (definition != null)
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

			string fullname = ScriptExportMonoType.GetFullName(type);
			if (m_types.TryGetValue(fullname, out ScriptExportType exportType))
			{
				return exportType;
			}
			return CreateType(type);
		}

		public ScriptExportArray RetrieveArray(TypeReference array)
		{
			string fullname = ScriptExportMonoType.GetFullName(array);
			if (m_arrays.TryGetValue(fullname, out ScriptExportArray exportArray))
			{
				return exportArray;
			}
			return CreateArray(array);
		}
		
		public ScriptExportPointer RetrievePointer(TypeReference pointer)
		{
			string fullname = ScriptExportMonoType.GetFullName(pointer);
			if (m_pointers.TryGetValue(fullname, out ScriptExportPointer exportArray))
			{
				return exportArray;
			}
			return CreatePointer(pointer);
		}

		public ScriptExportGeneric RetrieveGeneric(TypeReference generic)
		{
			string fullname = ScriptExportMonoType.GetFullName(generic);
			if (m_generic.TryGetValue(fullname, out ScriptExportGeneric exportGeneric))
			{
				return exportGeneric;
			}
			return CreateGeneric(generic);
		}

		public ScriptExportEnum RetrieveEnum(TypeDefinition @enum)
		{
			string fullname = ScriptExportMonoType.GetFullName(@enum);
			if (m_enums.TryGetValue(fullname, out ScriptExportEnum exportEnum))
			{
				return exportEnum;
			}
			return CreateEnum(@enum);
		}

		public ScriptExportDelegate RetrieveDelegate(TypeDefinition @delegate)
		{
			string fullname = ScriptExportMonoType.GetFullName(@delegate);
			if (m_delegates.TryGetValue(fullname, out ScriptExportDelegate exportDelegate))
			{
				return exportDelegate;
			}
			return CreateDelegate(@delegate);
		}

		public ScriptExportAttribute RetrieveAttribute(CustomAttribute attribute)
		{
			string fullname = ScriptExportMonoAttribute.ToFullName(attribute);
			if (m_attributes.TryGetValue(fullname, out ScriptExportAttribute exportAttribute))
			{
				return exportAttribute;
			}
			return CreateAttribute(attribute);
		}

		public ScriptExportMethod RetrieveMethod(MethodDefinition method)
		{
			ScriptExportMethod exportMethod = new ScriptExportMonoMethod(method);
			exportMethod.Init(this);
			return exportMethod;
		}

		public ScriptExportConstructor RetrieveConstructor(MethodDefinition constructor)
		{
			ScriptExportConstructor exportConstructor = new ScriptExportMonoConstructor(constructor);
			exportConstructor.Init(this);
			return exportConstructor;
		}

		public ScriptExportProperty RetrieveProperty(PropertyDefinition property)
		{
			ScriptExportProperty exportProperty = new ScriptExportMonoProperty(property);
			exportProperty.Init(this);
			return exportProperty;
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

		private ScriptExportType CreateType(TypeReference type, bool isUnique = true)
		{
			ScriptExportType exportType = new ScriptExportMonoType(type);
			if (isUnique)
			{
				m_types.Add(exportType.FullName, exportType);
			}
			exportType.Init(this);
			return exportType;
		}

		public ScriptExportArray CreateArray(TypeReference type)
		{
			ScriptExportArray exportArray = new ScriptExportMonoArray(type);
			m_arrays.Add(exportArray.FullName, exportArray);
			exportArray.Init(this);
			return exportArray;
		}
		
		public ScriptExportPointer CreatePointer(TypeReference type)
		{
			ScriptExportPointer exportPointer = new ScriptExportMonoPointer(type);
			m_pointers.Add(exportPointer.FullName, exportPointer);
			exportPointer.Init(this);
			return exportPointer;
		}

		public ScriptExportGeneric CreateGeneric(TypeReference type)
		{
			ScriptExportGeneric exportGeneric = new ScriptExportMonoGeneric(type);
			m_generic.Add(exportGeneric.FullName, exportGeneric);
			exportGeneric.Init(this);
			return exportGeneric;
		}

		private ScriptExportEnum CreateEnum(TypeReference @enum)
		{
			ScriptExportMonoEnum exportEnum = new ScriptExportMonoEnum(@enum);
			m_enums.Add(exportEnum.FullName, exportEnum);
			exportEnum.Init(this);
			return exportEnum;
		}

		private ScriptExportDelegate CreateDelegate(TypeDefinition @delegate)
		{
			ScriptExportMonoDelegate exportDelegate = new ScriptExportMonoDelegate(@delegate);
			m_delegates.Add(exportDelegate.FullName, exportDelegate);
			exportDelegate.Init(this);
			return exportDelegate;
		}

		private ScriptExportAttribute CreateAttribute(CustomAttribute attribute)
		{
			ScriptExportMonoAttribute exportAttribute = new ScriptExportMonoAttribute(attribute);
			m_attributes.Add(exportAttribute.FullName, exportAttribute);
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
			foreach (ScriptExportDelegate @delegate in exportType.NestedDelegates)
			{
				m_exported.Add(@delegate.FullName);
			}

			foreach (ScriptExportType nestedType in exportType.NestedTypes)
			{
				AddExportedType(nestedType);
			}
		}

		private static string ToUniqueFileName(string filePath)
		{
			if (FileUtils.Exists(filePath))
			{
				string directory = Path.GetDirectoryName(filePath);
				string fileName = Path.GetFileNameWithoutExtension(filePath);
				string fileExtension = Path.GetExtension(filePath);
				for (int i = 2; i < int.MaxValue; i++)
				{
					string newFilePath = Path.Combine(directory, $"{fileName}.{i}{fileExtension}");
					if (!FileUtils.Exists(newFilePath))
					{
						Logger.Log(LogType.Warning, LogCategory.Export, $"Found duplicate script file at {filePath}. Renamed to {newFilePath}");
						return newFilePath;
					}
				}
				throw new Exception($"Can't create unit file at {filePath}");
			}
			else
			{
				return filePath;
			}
		}

		private static bool IsBuiltInType(ScriptExportType type)
		{
			return IsBuiltinLibrary(type.Module);
		}

		public AssetLayout Layout { get; }

		public IEnumerable<ScriptExportType> Types => m_types.Values;
		public IEnumerable<ScriptExportEnum> Enums => m_enums.Values;
		public IEnumerable<ScriptExportDelegate> Delegates => m_delegates.Values;

		private const string MSCoreLibName = "mscorlib";
		private const string NetStandardName = "netstandard";
		private const string SystemName = "System";
		private const string CLRName = "CommonLanguageRuntimeLibrary";
		private const string UnityEngineName = "UnityEngine";
		private const string BooName = "Boo";
		private const string BooLangName = "Boo.Lang";
		private const string UnityScriptName = "UnityScript";
		private const string UnityScriptLangName = "UnityScript.Lang";
		private const string MonoName = "Mono";

		private readonly Dictionary<string, ScriptExportType> m_types = new Dictionary<string, ScriptExportType>();
		private readonly Dictionary<string, ScriptExportArray> m_arrays = new Dictionary<string, ScriptExportArray>();
		private readonly Dictionary<string, ScriptExportPointer> m_pointers = new Dictionary<string, ScriptExportPointer>();
		private readonly Dictionary<string, ScriptExportGeneric> m_generic = new Dictionary<string, ScriptExportGeneric>();
		private readonly Dictionary<string, ScriptExportEnum> m_enums = new Dictionary<string, ScriptExportEnum>();
		private readonly Dictionary<string, ScriptExportDelegate> m_delegates = new Dictionary<string, ScriptExportDelegate>();
		private readonly Dictionary<string, ScriptExportAttribute> m_attributes = new Dictionary<string, ScriptExportAttribute>();

		private readonly HashSet<string> m_exported = new HashSet<string>();

		private readonly string m_exportPath;
	}
}
