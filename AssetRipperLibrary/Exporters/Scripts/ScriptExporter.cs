using AssetRipper.Core;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Core.Structure.Assembly.Mono;
using AssetRipper.Core.Utils;
using AssetRipper.Library.Configuration;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetRipper.Library.Exporters.Scripts
{
	public class ScriptExporter : IAssetExporter
	{
		public ScriptExporter(IAssemblyManager assemblyManager, LibraryConfiguration configuration)
		{
			AssemblyManager = assemblyManager;
			ScriptExportMode = configuration.ScriptExportMode;
			LanguageVersion = configuration.ScriptLanguageVersion.ToCSharpLanguageVersion(configuration.Version);
			ScriptContentLevel = configuration.ScriptContentLevel;
			Enabled = ScriptExportMode == ScriptExportMode.Decompiled && ScriptContentLevel > ScriptContentLevel.Level0;
		}

		private IAssemblyManager AssemblyManager { get; }
		private ScriptExportMode ScriptExportMode { get; }
		private ICSharpCode.Decompiler.CSharp.LanguageVersion LanguageVersion { get; }
		private ScriptContentLevel ScriptContentLevel { get; }
		public bool Enabled { get; }
		private ScriptDecompiler Decompiler { get; set; }

		private string m_exportPath;

		private readonly Dictionary<string, TypeDefinition> m_types = new Dictionary<string, TypeDefinition>();
		private readonly HashSet<string> m_exported = new HashSet<string>();
		private static readonly UTF8Encoding utf8Encoding = new UTF8Encoding(false);
		private static readonly string[] specialTypeNames = new string[] { "<Module>", "<PrivateImplementationDetails>", "Consts", "Locale" };
		private static readonly string[] forbiddenNamespaces = new string[] { "Unity", "UnityEngine", "TMPro", "System", "Microsoft", "Mono" };
		private static readonly string[] forbiddenAssemblies = new string[] { "mscorlib" };


		public bool IsHandle(IUnityObjectBase asset) => Enabled && asset is IMonoScript;

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new ScriptExportCollection(this, (IMonoScript)asset);
		}

		public bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			throw new NotSupportedException("Need to export all scripts at once");
		}

		public void Export(IExportContainer container, IUnityObjectBase asset, string path, Action<IExportContainer, IUnityObjectBase, string> callback)
		{
			throw new NotSupportedException("Need to export all scripts at once");
		}

		public bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string dirPath)
		{
			Export(container, assets, dirPath, null);
			return true;
		}

		public AssetType ToExportType(IUnityObjectBase asset) => AssetType.Meta;

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}

		public void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string dirPath, Action<IExportContainer, IUnityObjectBase, string> callback)
		{
			Logger.Info(LogCategory.Export, "Exporting scripts...");

			if (string.IsNullOrEmpty(dirPath))
				throw new ArgumentNullException(nameof(dirPath));
			m_exportPath = dirPath;

			Decompiler = new ScriptDecompiler(AssemblyManager);
			Decompiler.LanguageVersion = LanguageVersion;
			Decompiler.ScriptContentLevel = ScriptContentLevel;

			AddTypes(AssemblyManager);
			ExportPrimaryScripts(container, assets, callback);
			ExportSecondaryScripts();
		}

		private void ExportPrimaryScripts(IExportContainer container, IEnumerable<IUnityObjectBase> assets, Action<IExportContainer, IUnityObjectBase, string> callback)
		{
			Dictionary<IUnityObjectBase, TypeDefinition> exportTypes = new Dictionary<IUnityObjectBase, TypeDefinition>();
			foreach (IUnityObjectBase asset in assets)
			{
				IMonoScript script = (IMonoScript)asset;
				TypeDefinition exportType = script.GetTypeDefinition();
				exportTypes.Add(asset, exportType);
			}
			int primaryTotal = exportTypes.Count;
			int count = 0;
			Logger.Info(LogCategory.Export, $"Exporting {primaryTotal} primary scripts...");
			foreach (KeyValuePair<IUnityObjectBase, TypeDefinition> exportType in exportTypes)
			{
				string path = Export(exportType.Value);
				if (path != null)
				{
					callback?.Invoke(container, exportType.Key, path);
				}
				count++;
				if (count % 100 == 0)
				{
					Logger.Info(LogCategory.Export, $"Exported {count}/{primaryTotal} primary scripts");
				}
			}
			Logger.Info(LogCategory.Export, "Primary script export finished.");
		}

		private void ExportSecondaryScripts()
		{
			var secondaryScripts = m_types.Values.Where(type => type.DeclaringType == null && !m_exported.Contains(type.FullName)).ToArray();
			int secondaryTotal = secondaryScripts.Length;
			int count = 0;
			Logger.Info(LogCategory.Export, $"Exporting {secondaryTotal} secondary scripts...");
			foreach (TypeDefinition type in secondaryScripts)
			{
				Export(type);
				count++;
				if (count % 100 == 0)
				{
					Logger.Info(LogCategory.Export, $"Exported {count}/{secondaryTotal} secondary scripts");
				}
			}
			Logger.Info(LogCategory.Export, "Secondary script export finished.");
		}

		private void AddTypes(IAssemblyManager assemblyManager)
		{
			foreach (var assembly in assemblyManager.GetAssemblies())
			{
				if (forbiddenAssemblies.Contains(assembly.Name.Name))
					continue;
				foreach (var module in assembly.Modules)
				{
					foreach (var type in module.Types)
					{
						if (!specialTypeNames.Contains(type.FullName) && !IsForbiddenNamespace(type.Namespace) && !m_types.ContainsKey(type.FullName))
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
			typeName = typeName.Replace('`', '.');
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

			try
			{
				string decompiledText = Decompiler.Decompile(exportType);
				if (string.IsNullOrEmpty(decompiledText))
				{
					Logger.Error(LogCategory.Export, $"Decompiling {exportType.FullName} returned an empty string");
				}
				else
				{
					string subPath = GetExportSubPath(exportType);
					string filePath = Path.Combine(m_exportPath, subPath);
					string uniqueFilePath = ToUniqueFileName(filePath);
					string directory = Path.GetDirectoryName(uniqueFilePath);
					Directory.CreateDirectory(directory);
					File.WriteAllText(uniqueFilePath, decompiledText, utf8Encoding);
					AddExportedType(exportType);
					return uniqueFilePath;
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"Error while decompiling {exportType.FullName}:", ex);
			}
			AddExportedType(exportType);
			return null;
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
						Logger.Warning(LogCategory.Export, $"Found duplicate script file at {filePath}. Renamed to {newFilePath}");
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
