using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Core.Structure.Assembly;
using AssetRipper.Core.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_1030;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetRipper.Library.Exporters.Scripts
{
	public class AssemblyExportCollection : ExportCollection
	{
		public override IAssetExporter AssetExporter { get; }
		public override ISerializedFile File { get; }
		public override IEnumerable<IUnityObjectBase> Assets => m_scripts.Keys;
		public override string Name => nameof(ScriptExportCollection);

		private static readonly UnityGUID UnityEngineGUID = new UnityGUID(0x1F55507F, 0xA1948D44, 0x4080F528, 0xC176C90E);

		private readonly List<IMonoScript> m_export = new List<IMonoScript>();
		private readonly HashSet<IMonoScript> m_unique = new HashSet<IMonoScript>();
		private readonly Dictionary<IUnityObjectBase, IMonoScript> m_scripts = new Dictionary<IUnityObjectBase, IMonoScript>();
		private readonly Dictionary<string, long> ScriptId = new Dictionary<string, long>();
		private readonly Dictionary<string, UnityGUID> AssemblyHash = new Dictionary<string, UnityGUID>();


		public AssemblyExportCollection(IAssetExporter assetExporter, IMonoScript script)
		{
			AssetExporter = assetExporter ?? throw new ArgumentNullException(nameof(assetExporter));

			File = script.SerializedFile;

			// find copies in whole project and skip them
			foreach (IUnityObjectBase asset in script.SerializedFile.Collection.FetchAssets())
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
					if (assetScript.IsScriptPresents())
					{
						m_export.Add(assetScript);
					}
				}
			}
		}

		public override bool Export(IProjectAssetContainer container, string projectDirectory)
		{
			if (m_export.Count == 0)
			{
				return false;
			}

			Logger.Info(LogCategory.Export, "Exporting scripts...");

			string pluginsFolder = Path.Combine(projectDirectory, AssetsKeyword, "Plugins");

			foreach (Mono.Cecil.AssemblyDefinition? assembly in ((AssemblyDllExporter)AssetExporter).AssemblyManager.GetAssemblies())
			{
				string assemblyName = assembly.Name.Name;
				if (!assemblyName.EndsWith(".dll"))
				{
					assemblyName = assemblyName + ".dll";
				}

				if (ReferenceAssemblies.IsReferenceAssembly(assemblyName))
				{
					continue;
				}

				string path = Path.Combine(pluginsFolder, assemblyName);
				Directory.CreateDirectory(pluginsFolder);
				using FileStream file = System.IO.File.Create(path);
				assembly.Write(file);
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
			if (!MonoScriptExtensions.HasAssemblyName(script.SerializedFile.Version, script.SerializedFile.Flags) || ReferenceAssemblies.IsUnityEngineAssembly(script.GetAssemblyNameFixed()))
			{
				if (MonoScriptExtensions.HasNamespace(script.SerializedFile.Version))
				{
					int fileID = Compute(script.Namespace_C115.String, script.ClassName_C115.String);
					return new MetaPtr(fileID, UnityEngineGUID, AssetExporter.ToExportType(asset));
				}
				else
				{
					ScriptIdentifier scriptInfo = script.GetScriptID();
					if (!scriptInfo.IsDefault)
					{
						int fileID = Compute(scriptInfo.Namespace, scriptInfo.Name);
						return new MetaPtr(fileID, UnityEngineGUID, AssetExporter.ToExportType(asset));
					}
				}
			}

			string? scriptKey = $"{script.AssemblyName_C115.String}{script.Namespace_C115.String}{script.ClassName_C115.String}";
			if (!ScriptId.ContainsKey(scriptKey))
			{
				ScriptId[scriptKey] = Compute(script.Namespace_C115.String, script.ClassName_C115.String);
			}

			return new MetaPtr(ScriptId[scriptKey], GetAssemblyGuid(script.AssemblyName_C115.String), AssetExporter.ToExportType(asset));
		}

		private static int Compute(string @namespace, string name)
		{
			string toBeHashed = $"s\0\0\0{@namespace}{name}";
			using MD4 hash = new();
			byte[] hashed = hash.ComputeHash(Encoding.UTF8.GetBytes(toBeHashed));

			int result = 0;
			for (int i = 3; i >= 0; --i)
			{
				result <<= 8;
				result |= hashed[i];
			}

			return result;
		}

		public UnityGUID GetAssemblyGuid(string assemblyName)
		{
			if (AssemblyHash.TryGetValue(assemblyName, out UnityGUID result))
			{
				return result;
			}
			else
			{
				UnityGUID guid = CalculateAssemblyHashGuid(assemblyName);
				AssemblyHash[assemblyName] = guid;
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
			IDefaultImporter importer = DefaultImporterFactory.CreateAsset(container.ExportVersion);//Might need to use PluginImporter
			Meta meta = new Meta(guid, importer);
			ExportMeta(container, meta, path);
		}
	}
}
