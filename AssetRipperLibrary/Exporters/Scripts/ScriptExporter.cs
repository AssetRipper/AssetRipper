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
			ScriptExportMode = configuration.ScriptExportMode;
			Decompiler = new ScriptDecompiler(AssemblyManager);
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

				// assembly definitions were added in 2017.3
				//     see: https://blog.unity.com/technology/unity-2017-3b-feature-preview-assembly-definition-files-and-transform-tool
				if (container.ExportVersion.IsGreaterEqual(2017, 3) && 
					// exclude predefined assemblies like Assembly-CSharp.dll
					//    see: https://docs.unity3d.com/2017.3/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html
					!ReferenceAssemblies.IsPredefinedAssembly(assembly.Name.Name))
				{
					AssemblyDefinitionExporter.Export(assembly, outputDirectory);
				}
			}
			if(callback is not null)
			{
				foreach (IMonoScript asset in assets.Cast<IMonoScript>())
				{
					if (ScriptExportCollection.IsEngineScript(asset))
					{
						continue;
					}

					string filePath = Path.Combine(dirPath, GetExportSubPath(asset.GetTypeDefinition()));
					if (!File.Exists(filePath))
					{
						Logger.Error(LogCategory.Export, $"No script exists at {filePath}");
					}
					if (File.Exists($"{filePath}.meta"))
					{
						Logger.Error(LogCategory.Export, $"Metafile already exists at {filePath}.meta");
					}
					callback.Invoke(container, asset, filePath);
				}
			}
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
				typeName = typeName.Substring(0, index);
			}
			index = typeName.IndexOf('`');
			if (index >= 0)
			{
				typeName = typeName.Substring(0, index);
			}
			return GetExportSubPath(type.Module.Name, type.Namespace, typeName);
		}
	}
}
