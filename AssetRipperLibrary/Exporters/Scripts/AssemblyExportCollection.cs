using AssetRipper.Core;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Meta.Importers;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure.Assembly;
using AssetRipper.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Library.Exporters.Scripts
{
	public class AssemblyExportCollection : ExportCollection
	{
		public override IAssetExporter AssetExporter { get; }
		public override ISerializedFile File { get; }
		public override IEnumerable<UnityObjectBase> Assets => m_scripts.Keys;
		public override string Name => nameof(ScriptExportCollection);

		private static readonly UnityGUID UnityEngineGUID = new UnityGUID(0x1F55507F, 0xA1948D44, 0x4080F528, 0xC176C90E);
		private static readonly Regex s_unityEngine = new Regex(@"^UnityEngine(\.[0-9a-zA-Z]+)*(\.dll)?$", RegexOptions.Compiled);

		private readonly List<MonoScript> m_export = new List<MonoScript>();
		private readonly HashSet<MonoScript> m_unique = new HashSet<MonoScript>();
		private readonly Dictionary<UnityObjectBase, MonoScript> m_scripts = new Dictionary<UnityObjectBase, MonoScript>();
		private readonly Dictionary<string, long> ScriptId = new Dictionary<string, long>();
		private readonly Dictionary<string, UnityGUID> AssemblyHash = new Dictionary<string, UnityGUID>();


		public AssemblyExportCollection(IAssetExporter assetExporter, MonoScript script)
		{
			AssetExporter = assetExporter ?? throw new ArgumentNullException(nameof(assetExporter));

			File = script.File;

			// find copies in whole project and skip them
			foreach (UnityObjectBase asset in script.File.Collection.FetchAssets())
			{
				if (asset.ClassID != ClassIDType.MonoScript)
				{
					continue;
				}

				MonoScript assetScript = (MonoScript)asset;
				MonoScript unique = assetScript;
				foreach (MonoScript export in m_unique)
				{
					if (assetScript.ClassName != export.ClassName)
					{
						continue;
					}
					if (assetScript.Namespace != export.Namespace)
					{
						continue;
					}
					if (assetScript.AssemblyName != export.AssemblyName)
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

		public override bool Export(IProjectAssetContainer container, string dirPath)
		{
			if (m_export.Count == 0)
			{
				return false;
			}

			Logger.Info(LogCategory.Export, "Exporting scripts...");

			string scriptFolder = m_export[0].ExportPath;
			string scriptPath = Path.Combine(dirPath, scriptFolder);

			foreach (var assembly in ((AssemblyDllExporter)AssetExporter).AssemblyManager.GetAssemblies())
			{
				string assemblyName = assembly.Name.Name;
				if (!assemblyName.EndsWith(".dll"))
					assemblyName = assemblyName + ".dll";

				if (IsReferenceAssembly(assemblyName))
					continue;

				string path = System.IO.Path.Combine(scriptPath, assemblyName);
				DirectoryUtils.CreateVirtualDirectory(scriptPath);
				using var file = FileUtils.CreateVirtualFile(path);
				assembly.Write(file);
				OnAssemblyExported(container, path);
			}
			Logger.Info(LogCategory.Export, "Finished exporting scripts");
			return true;
		}

		private static bool IsReferenceAssembly(string assemblyName)
		{
			if (assemblyName == null)
				throw new ArgumentNullException(assemblyName);
			if (assemblyName.StartsWith("System."))
				return true;
			if (assemblyName.StartsWith("Unity."))
				return true;
			if (assemblyName.StartsWith("UnityEngine."))
				return true;
			if (assemblyName.StartsWith("UnityEditor."))
				return true;
			if (assemblyName == "mscorlib.dll")
				return true;
			if (assemblyName == "netstandard.dll")
				return true;
			if (assemblyName == "Mono.Security.dll")
				return true;
			return false;
		}

		public override bool IsContains(UnityObjectBase asset)
		{
			return m_scripts.ContainsKey(asset);
		}

		public override long GetExportID(UnityObjectBase asset)
		{
			return ExportIdHandler.GetMainExportID(asset);
		}

		public override MetaPtr CreateExportPointer(UnityObjectBase asset, bool isLocal)
		{
			if (isLocal)
			{
				throw new NotSupportedException();
			}

			MonoScript script = m_scripts[asset];
			if (!MonoScript.HasAssemblyName(script.File.Version, script.File.Flags) || s_unityEngine.IsMatch(script.AssemblyName))
			{
				if (MonoScript.HasNamespace(script.File.Version))
				{
					int fileID = Compute(script.Namespace, script.ClassName);
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

			var scriptKey = $"{script.AssemblyNameOrigin}{script.Namespace}{script.ClassName}";
			if (!ScriptId.ContainsKey(scriptKey))
				ScriptId[scriptKey] = Compute(script.Namespace, script.ClassName);

			return new MetaPtr(ScriptId[scriptKey], GetAssemblyGuid(script.AssemblyNameOrigin), AssetExporter.ToExportType(asset));
		}

		private static int Compute(string @namespace, string name)
		{
			string toBeHashed = $"s\0\0\0{@namespace}{name}";
			using (HashAlgorithm hash = new MD4())
			{
				byte[] hashed = hash.ComputeHash(Encoding.UTF8.GetBytes(toBeHashed));

				int result = 0;
				for (int i = 3; i >= 0; --i)
				{
					result <<= 8;
					result |= hashed[i];
				}

				return result;
			}
		}

		public UnityGUID GetAssemblyGuid(string assemblyName)
		{
			if (AssemblyHash.TryGetValue(assemblyName, out UnityGUID result))
			{
				return result;
			}
			else
			{
				var guid = CalculateAssemblyHashGuid(assemblyName);
				AssemblyHash[assemblyName] = guid;
				return guid;
			}
		}

		private static UnityGUID CalculateAssemblyHashGuid(string assemblyPath)
		{
			using (var md5 = MD5.Create())
			{
				string shortName = Path.GetFileNameWithoutExtension(assemblyPath);
				byte[] shortNameBytes = Encoding.Default.GetBytes(shortName);
				var shortNameHash = md5.ComputeHash(shortNameBytes);
				return new UnityGUID(shortNameHash);
			}
		}

		private void OnAssemblyExported(IExportContainer container, string path)
		{
			var guid = GetAssemblyGuid(Path.GetFileName(path));
			DefaultImporter importer = new DefaultImporter(container.ExportLayout);//Might need to use PluginImporter
			Meta meta = new Meta(guid, importer);
			ExportMeta(container, meta, path);
		}
	}
}