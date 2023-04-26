using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Utils;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated.Classes.ClassID_1030;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using System.Text;

namespace AssetRipper.Export.UnityProjects.Scripts
{
	public class AssemblyExportCollection : ExportCollection
	{
		public override AssemblyDllExporter AssetExporter { get; }
		public override AssetCollection File { get; }
		public override IEnumerable<IUnityObjectBase> Assets => m_scripts.Keys;
		public override string Name => nameof(ScriptExportCollection);

		private static readonly UnityGUID UnityEngineGUID = new UnityGUID(0x1F55507F, 0xA1948D44, 0x4080F528, 0xC176C90E);

		private readonly List<IMonoScript> m_export = new List<IMonoScript>();
		private readonly HashSet<IMonoScript> m_unique = new HashSet<IMonoScript>();
		private readonly Dictionary<IUnityObjectBase, IMonoScript> m_scripts = new Dictionary<IUnityObjectBase, IMonoScript>();
		private readonly Dictionary<string, long> ScriptId = new Dictionary<string, long>();
		private readonly Dictionary<string, UnityGUID> AssemblyHash = new Dictionary<string, UnityGUID>();


		public AssemblyExportCollection(AssemblyDllExporter assetExporter, IMonoScript script)
		{
			AssetExporter = assetExporter ?? throw new ArgumentNullException(nameof(assetExporter));

			File = script.Collection;

			// find copies in whole project and skip them
			foreach (IUnityObjectBase asset in script.Collection.Bundle.FetchAssets())
			{
				if (asset is not IMonoScript assetScript)
				{
					continue;
				}

				IMonoScript unique = assetScript;
				foreach (IMonoScript export in m_unique)
				{
					if (assetScript.ClassName_C115 != export.ClassName_C115)
					{
						continue;
					}
					if (assetScript.Namespace_C115 != export.Namespace_C115)
					{
						continue;
					}
					if (assetScript.GetAssemblyNameFixed() != export.GetAssemblyNameFixed())
					{
						continue;
					}

					unique = export;
					break;
				}

				m_scripts.Add(assetScript, unique);
				if (assetScript == unique)
				{
					m_unique.Add(assetScript);
					if (assetScript.IsScriptPresents(AssetExporter.AssemblyManager))
					{
						m_export.Add(assetScript);
					}
				}
			}
		}

		public override bool Export(IExportContainer container, string projectDirectory)
		{
			if (m_export.Count == 0)
			{
				return false;
			}

			Logger.Info(LogCategory.Export, "Exporting scripts...");

			string pluginsFolder = Path.Combine(projectDirectory, AssetsKeyword, "Plugins");

			foreach (AsmResolver.DotNet.AssemblyDefinition? assembly in ((AssemblyDllExporter)AssetExporter).AssemblyManager.GetAssemblies())
			{
				string assemblyName = assembly.Name!;
				if (!assemblyName.EndsWith(".dll"))
				{
					assemblyName += ".dll";
				}

				if (ReferenceAssemblies.IsReferenceAssembly(assemblyName))
				{
					continue;
				}

				string path = Path.Combine(pluginsFolder, assemblyName);
				Directory.CreateDirectory(pluginsFolder);
				assembly.Write(path);
				OnAssemblyExported(container, path);
			}
			Logger.Info(LogCategory.Export, "Finished exporting scripts");
			return true;
		}

		public override bool IsContains(IUnityObjectBase asset)
		{
			return m_scripts.ContainsKey(asset);
		}

		public override long GetExportID(IUnityObjectBase asset)
		{
			return ExportIdHandler.GetMainExportID(asset);
		}

		public override MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal)
		{
			if (isLocal)
			{
				throw new NotSupportedException();
			}

			IMonoScript script = m_scripts[asset];
			if (!MonoScriptExtensions.HasAssemblyName(script.Collection.Version, script.Collection.Flags) || ReferenceAssemblies.IsUnityEngineAssembly(script.GetAssemblyNameFixed()))
			{
				if (MonoScriptExtensions.HasNamespace(script.Collection.Version))
				{
					int fileID = ScriptHashing.CalculateScriptFileID(script.Namespace_C115.Data, script.ClassName_C115.Data);
					return new MetaPtr(fileID, UnityEngineGUID, AssetExporter.ToExportType(asset));
				}
				else
				{
					ScriptIdentifier scriptInfo = script.GetScriptID(AssetExporter.AssemblyManager);
					if (!scriptInfo.IsDefault)
					{
						int fileID = ScriptHashing.CalculateScriptFileID(scriptInfo.Namespace, scriptInfo.Name);
						return new MetaPtr(fileID, UnityEngineGUID, AssetExporter.ToExportType(asset));
					}
				}
			}

			string? scriptKey = $"{script.AssemblyName_C115.String}{script.Namespace_C115.String}{script.ClassName_C115.String}";
			if (!ScriptId.ContainsKey(scriptKey))
			{
				ScriptId[scriptKey] = ScriptHashing.CalculateScriptFileID(script.Namespace_C115.Data, script.ClassName_C115.Data);
			}

			return new MetaPtr(ScriptId[scriptKey], GetAssemblyGuid(script.AssemblyName_C115.String), AssetExporter.ToExportType(asset));
		}

		public UnityGUID GetAssemblyGuid(string assemblyName)
		{
			if (AssemblyHash.TryGetValue(assemblyName, out UnityGUID result))
			{
				return result;
			}
			else
			{
				//MonoScripts don't always have the .dll extension.
				string assemblyNameWithExtension = assemblyName.EndsWith(".dll", StringComparison.Ordinal) ? assemblyName : assemblyName + ".dll";
				UnityGUID guid = CalculateAssemblyHashGuid(assemblyNameWithExtension);
				AssemblyHash[assemblyName] = guid;
				AssemblyHash[assemblyNameWithExtension] = guid;
				return guid;
			}
		}

		private static UnityGUID CalculateAssemblyHashGuid(string assemblyPath)
		{
			string shortName = Path.GetFileNameWithoutExtension(assemblyPath);
			return UnityGUID.Md5Hash(shortName);
		}

		private void OnAssemblyExported(IExportContainer container, string path)
		{
			UnityGUID guid = GetAssemblyGuid(Path.GetFileName(path));
			IDefaultImporter importer = DefaultImporterFactory.CreateAsset(container.ExportVersion, container.File);//Might need to use PluginImporter
			Meta meta = new Meta(guid, importer);
			ExportMeta(container, meta, path);
		}
	}
}
