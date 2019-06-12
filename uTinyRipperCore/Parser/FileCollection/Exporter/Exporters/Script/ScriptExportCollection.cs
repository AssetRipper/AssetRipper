using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters.Classes;
using uTinyRipper.Classes;
using uTinyRipper.Importers;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.AssetExporters
{
	public class ScriptExportCollection : ExportCollection
	{
		public ScriptExportCollection(IAssetExporter assetExporter, MonoScript script)
		{
			if (assetExporter == null)
			{
				throw new ArgumentNullException(nameof(assetExporter));
			}
			AssetExporter = assetExporter;

			File = script.File;

			// find copies in whole project and skip them
			foreach (Object asset in script.File.Collection.FetchAssets())
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

		public override bool Export(ProjectAssetContainer container, string dirPath)
		{
			if (m_export.Count == 0)
			{
				return false;
			}

			string scriptFolder = m_export[0].ExportPath;
			string scriptPath = Path.Combine(dirPath, scriptFolder);

			AssetExporter.Export(container, m_export, scriptPath, OnScriptExported);
			return true;
		}

		public override bool IsContains(Object asset)
		{
			return m_scripts.ContainsKey(asset);
		}

		public override long GetExportID(Object asset)
		{
			return GetMainExportID(asset);
		}

		public override ExportPointer CreateExportPointer(Object asset, bool isLocal)
		{
			if (isLocal)
			{
				throw new NotSupportedException();
			}

			MonoScript script = m_scripts[asset];
			if (!MonoScript.IsReadAssemblyName(script.File.Version, script.File.Flags) || s_unityEngine.IsMatch(script.AssemblyName))
			{
				if (MonoScript.IsReadNamespace(script.File.Version))
				{
					int fileID = Compute(script.Namespace, script.ClassName);
					return new ExportPointer(fileID, UnityEngineGUID, AssetExporter.ToExportType(asset));
				}
				else
				{
					ScriptIdentifier scriptInfo = script.GetScriptID();
					if (!scriptInfo.IsDefault)
					{
						int fileID = Compute(scriptInfo.Namespace, scriptInfo.Name);
						return new ExportPointer(fileID, UnityEngineGUID, AssetExporter.ToExportType(asset));
					}
				}
			}

			long exportID = GetExportID(asset);
			EngineGUID uniqueGUID = script.GUID;
			return new ExportPointer(exportID, uniqueGUID, AssetExporter.ToExportType(asset));
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

		private void OnScriptExported(IExportContainer container, Object asset, string path)
		{
			MonoScript script = (MonoScript)asset;
			MonoImporter importer = new MonoImporter(script);
			Meta meta = new Meta(importer, script.GUID);
			ExportMeta(container, meta, path);
		}

		public override IAssetExporter AssetExporter { get; }
		public override ISerializedFile File { get; }
		public override IEnumerable<Object> Assets => m_scripts.Keys;
		public override string Name => nameof(ScriptExportCollection);

		private static readonly EngineGUID UnityEngineGUID = new EngineGUID(0xE09C671C, 0x825f0804, 0x44d8491a, 0xf70555f1);
		private static readonly Regex s_unityEngine = new Regex(@"^UnityEngine(\.[0-9a-zA-Z]+)*(\.dll)?$", RegexOptions.Compiled);

		private readonly List<MonoScript> m_export = new List<MonoScript>();
		private readonly HashSet<MonoScript> m_unique = new HashSet<MonoScript>();
		private readonly Dictionary<Object, MonoScript> m_scripts = new Dictionary<Object, MonoScript>();
	}
}
