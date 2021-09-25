using AssetRipper.Core.IO;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Core.Structure.Assembly.Mono;
using AssetRipper.Core.Utils;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetRipper.Library.Exporters.Scripts
{
	public sealed class ScriptManager
	{
		ScriptDecompiler Decompiler { get; }

		public IEnumerable<TypeDefinition> Types => m_types.Values;

		private Dictionary<string, TypeDefinition> m_types { get; } 

		private readonly HashSet<string> m_exported = new HashSet<string>();

		private readonly string m_exportPath;

		private static readonly string[] specialTypeNames = new string[] { "<Module>", "<PrivateImplementationDetails>", "Consts", "Locale" };
		private static readonly string[] forbiddenNamespaces = new string[] { "Unity", "UnityEngine", "TMPro", "System", "Microsoft", "Mono"};
		private static readonly string[] forbiddenAssemblies = new string[] { "mscorlib" };

		public ScriptManager(IAssemblyManager assemblyManager, string exportPath)
		{
			if (string.IsNullOrEmpty(exportPath))
				throw new ArgumentNullException(nameof(exportPath));
			Decompiler = new ScriptDecompiler(assemblyManager);
			m_exportPath = exportPath;
			m_types = new Dictionary<string, TypeDefinition>();
			AddTypes(assemblyManager);
		}

		private void AddTypes(IAssemblyManager assemblyManager)
		{
			foreach(var assembly in assemblyManager.GetAssemblies())
			{
				if (forbiddenAssemblies.Contains(assembly.Name.Name))
					continue;
				foreach(var module in assembly.Modules)
				{
					foreach(var type in module.Types)
					{
						if(!specialTypeNames.Contains(type.FullName) && !IsForbiddenNamespace(type.Namespace) && !m_types.ContainsKey(type.FullName))
							m_types.Add(type.FullName, type);
					}
				}
			}
		}

		private static bool IsForbiddenNamespace(string @namespace)
		{
			return !string.IsNullOrEmpty(@namespace) && forbiddenNamespaces.Contains(@namespace.Split('.').FirstOrDefault());
		}

		private static string GetExportSubPath(string assembly, string @namespace, string @class)
		{
			string assemblyFolder = BaseManager.ToAssemblyName(assembly);
			string namespaceFolder = @namespace.Replace('.', Path.DirectorySeparatorChar);
			string folderPath = Path.Combine(assemblyFolder, namespaceFolder);
			string filePath = Path.Combine(folderPath, @class);
			return $"{DirectoryUtils.FixInvalidPathCharacters(filePath)}.cs";
		}

		private static string GetExportSubPath(TypeDefinition type)
		{
			string typeName = type.Name;
			int index = typeName.IndexOf('<');
			if (index >= 0)
			{
				string normalName = typeName.Substring(0, index);
				typeName = normalName + $".{typeName.Count(t => t == ',') + 1}";
			}
			return GetExportSubPath(type.Module.Name, type.Namespace, typeName);
		}

		public string Export(TypeDefinition exportType)
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
			if (!Directory.Exists(directory))
			{
				DirectoryUtils.CreateVirtualDirectory(directory);
			}

			try
			{
				string decompiledText = Decompiler.Decompile(exportType);
				using (Stream fileStream = FileUtils.CreateVirtualFile(uniqueFilePath))
				{
					using (StreamWriter writer = new InvariantStreamWriter(fileStream, new UTF8Encoding(false)))
					{
						writer.Write(decompiledText);
					}
				}
				AddExportedType(exportType);
				return uniqueFilePath;
			}
			catch(Exception ex)
			{
				Logger.Error($"Error while decompiling {exportType.FullName}:", ex);
				AddExportedType(exportType);
				return null;
			}
		}

		public void ExportRest()
		{
			foreach (TypeDefinition type in m_types.Values)
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
		}

		private void AddExportedType(TypeDefinition exportType)
		{
			m_exported.Add(exportType.FullName);

			foreach (TypeDefinition nestedType in exportType.NestedTypes)
			{
				AddExportedType(nestedType);
			}
		}

		private static string ToUniqueFileName(string filePath)
		{
			if (File.Exists(filePath))
			{
				string directory = Path.GetDirectoryName(filePath);
				string fileName = Path.GetFileNameWithoutExtension(filePath);
				string fileExtension = Path.GetExtension(filePath);
				for (int i = 2; i < int.MaxValue; i++)
				{
					string newFilePath = Path.Combine(directory, $"{fileName}.{i}{fileExtension}");
					if (!File.Exists(newFilePath))
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

		private static bool IsBuiltInType(TypeDefinition type)
		{
			return MonoUtils.IsBuiltinLibrary(type.Module.Name);
		}
	}
}