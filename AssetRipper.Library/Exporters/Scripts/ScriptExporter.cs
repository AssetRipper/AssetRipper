using AssetRipper.Core.Configuration;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Core.Utils;
using AssetRipper.Library.Configuration;
using AssetRipper.Library.Exporters.Scripts.AssemblyDefinitions;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetRipper.Library.Exporters.Scripts
{
	public class ScriptExporter : IAssetExporter
	{
		public ScriptExporter(IAssemblyManager assemblyManager, LibraryConfiguration configuration)
		{
			AssemblyManager = assemblyManager;
			Decompiler = new ScriptDecompiler(AssemblyManager);
			LanguageVersion = configuration.ScriptLanguageVersion.ToCSharpLanguageVersion(configuration.Version);
			ScriptContentLevel = configuration.ScriptContentLevel;
		}

		public IAssemblyManager AssemblyManager { get; }
		private ICSharpCode.Decompiler.CSharp.LanguageVersion LanguageVersion { get; }
		private ScriptContentLevel ScriptContentLevel { get; }
		private ScriptDecompiler Decompiler { get; set; }

		public bool IsHandle(IUnityObjectBase asset) => asset is IMonoScript;

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

		public bool ToUnknownExportType(Type type, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}

		public void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string dirPath, Action<IExportContainer, IUnityObjectBase, string>? callback)
		{
			Logger.Info(LogCategory.Export, "Exporting scripts...");

			if (string.IsNullOrEmpty(dirPath))
			{
				throw new ArgumentNullException(nameof(dirPath));
			}

			Dictionary<string, AssemblyDefinitionDetails> assemblyDefinitionDetailsDictionary = new();

			if (AssemblyManager.IsSet)
			{
				Decompiler.LanguageVersion = LanguageVersion;
				Decompiler.ScriptContentLevel = ScriptContentLevel;

				foreach (AssemblyDefinition assembly in AssemblyManager.GetAssemblies())
				{
					if (ReferenceAssemblies.IsReferenceAssembly(assembly.Name.Name))
					{
						continue;
					}

					Logger.Info(LogCategory.Export, $"Decompiling {assembly.Name.Name}");
					string outputDirectory = Path.Combine(dirPath, assembly.Name.Name);
					Directory.CreateDirectory(outputDirectory);
					Decompiler.DecompileWholeProject(assembly, outputDirectory);

					assemblyDefinitionDetailsDictionary.TryAdd(assembly.Name.Name, new AssemblyDefinitionDetails(assembly, outputDirectory));
				}
			}

			foreach (IMonoScript asset in assets.Cast<IMonoScript>())
			{
				if (ScriptExportCollection.IsEngineScript(asset))
				{
					continue;
				}

				GetExportSubPath(asset, out string subFolderPath, out string fileName);
				string folderPath = Path.Combine(dirPath, subFolderPath);
				string filePath = Path.Combine(folderPath, fileName);
				if (!File.Exists(filePath))
				{
					Directory.CreateDirectory(folderPath);
					File.WriteAllText(filePath, GetEmptyScriptContent(asset));
					string assemblyName = BaseManager.ToAssemblyName(asset.GetAssemblyNameFixed());
					assemblyDefinitionDetailsDictionary.TryAdd(assemblyName,
						new AssemblyDefinitionDetails(assemblyName, Path.Combine(dirPath, assemblyName)));
				}

				if (callback is not null)
				{
					if (File.Exists($"{filePath}.meta"))
					{
						Logger.Error(LogCategory.Export, $"Metafile already exists at {filePath}.meta");
						//throw new Exception($"Metafile already exists at {filePath}.meta");
					}
					else
					{
						callback.Invoke(container, asset, filePath);
					}
				}
			}

			// assembly definitions were added in 2017.3
			//     see: https://blog.unity.com/technology/unity-2017-3b-feature-preview-assembly-definition-files-and-transform-tool
			if (assemblyDefinitionDetailsDictionary.Count > 0 && container.ExportVersion.IsGreaterEqual(2017, 3))
			{
				foreach (AssemblyDefinitionDetails details in assemblyDefinitionDetailsDictionary.Values)
				{
					// exclude predefined assemblies like Assembly-CSharp.dll
					//    see: https://docs.unity3d.com/2017.3/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html
					if (!ReferenceAssemblies.IsPredefinedAssembly(details.AssemblyName))
					{
						AssemblyDefinitionExporter.Export(details);
					}
				}
			}
		}

		private static void GetExportSubPath(string assembly, string @namespace, string @class, out string folderPath, out string fileName)
		{
			string assemblyFolder = BaseManager.ToAssemblyName(assembly);
			string namespaceFolder = @namespace.Replace('.', Path.DirectorySeparatorChar);
			folderPath = DirectoryUtils.FixInvalidPathCharacters(Path.Combine(assemblyFolder, namespaceFolder));
			fileName = $"{DirectoryUtils.FixInvalidPathCharacters(@class)}.cs";
		}

		private static void GetExportSubPath(IMonoScript script, out string folderPath, out string fileName)
		{
			GetExportSubPath(script.GetAssemblyNameFixed(), script.Namespace_C115.String, script.ClassName_C115.String, out folderPath, out fileName);
		}

		private static string GetEmptyScriptContent(IMonoScript script)
		{
			return GetEmptyScriptContent(script.Namespace_C115.String, script.ClassName_C115.String);
		}

		private static string GetEmptyScriptContent(string? @namespace, string name)
		{
			if (string.IsNullOrEmpty(@namespace))
			{
				return $@"using UnityEngine;

public class {name} : MonoBehaviour
{{
	//Dummy class. Use different settings or provide .NET dll files for better decompilation output
}}";
			}
			else
			{
				return $@"using UnityEngine;

namespace {@namespace}
{{
	public class {name} : MonoBehaviour
	{{
		//Dummy class. Use different settings or provide .NET dll files for better decompilation output
	}}
}}";
			}
		}
	}
}
