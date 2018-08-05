using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;
using UtinyRipper.SerializedFiles;
using UtinyRipper.Importers;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class ScriptCollection : ExportCollection
	{
		public ScriptCollection(IAssetExporter assetExporter, MonoScript script)
		{
			if (assetExporter == null)
			{
				throw new ArgumentNullException(nameof(assetExporter));
			}
			AssetExporter = assetExporter;

			File = script.File;

			// find copies in whole project and skip them
			foreach (ISerializedFile file in script.File.Collection.Files)
			{
				foreach(Object asset in file.FetchAssets())
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
					if(assetScript == unique)
					{
						m_unique.Add(assetScript);
						if (assetScript.IsScriptPresents())
						{
							m_export.Add(assetScript);
						}
					}
				}
			}
		}

		public override bool Export(ProjectAssetContainer container, string dirPath)
		{
			if(m_export.Count == 0)
			{
				return false;
			}

			string scriptFolder = m_export[0].ExportName;
			string scriptPath = Path.Combine(dirPath, scriptFolder);

			AssetExporter.Export(container, m_export, scriptPath, OnScriptExported);
			return true;
		}

		public override bool IsContains(Object asset)
		{
			return m_scripts.ContainsKey(asset);
		}

		public override ulong GetExportID(Object asset)
		{
			return GetMainExportID(asset);
		}

		public override ExportPointer CreateExportPointer(Object asset, bool isLocal)
		{
			if(isLocal)
			{
				throw new NotSupportedException();
			}

			ulong exportID = GetExportID(asset);
			EngineGUID uniqueGUID = m_scripts[asset].GUID;
			return new ExportPointer(exportID, uniqueGUID, AssetExporter.ToExportType(asset));
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
		public override string Name => nameof(ScriptCollection);

		private readonly List<MonoScript> m_export = new List<MonoScript>();
		private readonly HashSet<MonoScript> m_unique = new HashSet<MonoScript>();
		private readonly Dictionary<Object, Object> m_scripts = new Dictionary<Object, Object>();
	}
}
