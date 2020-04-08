using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.Game.Assembly;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Project
{
	public class ScriptExportCollection : ExportCollection
	{
		public ScriptExportCollection(IAssetExporter assetExporter, MonoScript script)
		{
			AssetExporter = assetExporter ?? throw new ArgumentNullException(nameof(assetExporter));

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

		public override MetaPtr CreateExportPointer(Object asset, bool isLocal)
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

			long exportID = GetExportID(asset);
			UnityGUID uniqueGUID = script.GUID;
			return new MetaPtr(exportID, uniqueGUID, AssetExporter.ToExportType(asset));
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
			MonoImporter importer = new MonoImporter(container.ExportLayout);
			importer.ExecutionOrder = (short)script.ExecutionOrder;
			Meta meta = new Meta(script.GUID, importer);
			ExportMeta(container, meta, path);
		}

		public override IAssetExporter AssetExporter { get; }
		public override ISerializedFile File { get; }
		public override IEnumerable<Object> Assets => m_scripts.Keys;
		public override string Name => nameof(ScriptExportCollection);

		private static readonly UnityGUID UnityEngineGUID = new UnityGUID(0x1F55507F, 0xA1948D44, 0x4080F528, 0xC176C90E);
		private static readonly Regex s_unityEngine = new Regex(@"^UnityEngine(\.[0-9a-zA-Z]+)*(\.dll)?$", RegexOptions.Compiled);

		private readonly List<MonoScript> m_export = new List<MonoScript>();
		private readonly HashSet<MonoScript> m_unique = new HashSet<MonoScript>();
		private readonly Dictionary<Object, MonoScript> m_scripts = new Dictionary<Object, MonoScript>();
	}
}
