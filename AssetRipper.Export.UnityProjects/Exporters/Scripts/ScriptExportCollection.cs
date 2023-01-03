using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Core.Structure.Assembly;
using AssetRipper.Core.Utils;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated.Classes.ClassID_1035;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetRipper.Library.Exporters.Scripts
{
	public partial class ScriptExportCollection : ExportCollection
	{
		public ScriptExportCollection(ScriptExporter assetExporter, IMonoScript script)
		{
			AssetExporter = assetExporter ?? throw new ArgumentNullException(nameof(assetExporter));
			File = script.Collection;

			// find copies in whole project and skip them
			Dictionary<MonoScriptInfo, IMonoScript> uniqueDictionary = new();
			foreach (IUnityObjectBase asset in script.Collection.Bundle.FetchAssetsInHierarchy())
			{
				if (asset is not IMonoScript assetScript)
				{
					continue;
				}

				MonoScriptInfo info = MonoScriptInfo.From(assetScript);
				if (uniqueDictionary.TryGetValue(info, out IMonoScript? uniqueScript))
				{
					m_scripts.Add(assetScript, uniqueScript);
				}
				else
				{
					m_scripts.Add(assetScript, assetScript);
					uniqueDictionary.Add(info, assetScript);
					if (!AssetExporter.AssemblyManager.IsSet || assetScript.IsScriptPresents(AssetExporter.AssemblyManager))
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

			string scriptsFolder = Path.Combine(projectDirectory, AssetsKeyword, "Scripts");

			AssetExporter.Export(container, m_export, scriptsFolder, OnScriptExported);
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
			if (IsEngineScript(script))
			{
				if (MonoScriptExtensions.HasNamespace(script.Collection.Version))
				{
					int fileID = Compute(script.Namespace_C115.String, script.ClassName_C115.String);
					return new MetaPtr(fileID, UnityEngineGUID, AssetExporter.ToExportType(asset));
				}
				else
				{
					ScriptIdentifier scriptInfo = script.GetScriptID(AssetExporter.AssemblyManager);
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

		private void OnScriptExported(IExportContainer container, IUnityObjectBase asset, string path)
		{
			IMonoScript script = (IMonoScript)asset;
			IMonoImporter importer = MonoImporterFactory.CreateAsset(container.ExportVersion);
			importer.ExecutionOrder_C1035 = (short)script.ExecutionOrder_C115;
			Meta meta = new Meta(script.GUID, importer);
			ExportMeta(container, meta, path);
		}

		public static bool IsEngineScript(IMonoScript script)
		{
			return ReferenceAssemblies.IsUnityEngineAssembly(script.GetAssemblyNameFixed());
		}

		public override ScriptExporter AssetExporter { get; }
		public override AssetCollection File { get; }
		public override IEnumerable<IUnityObjectBase> Assets => m_scripts.Keys;
		public override string Name => nameof(ScriptExportCollection);

		private static readonly UnityGUID UnityEngineGUID = new UnityGUID(0x1F55507F, 0xA1948D44, 0x4080F528, 0xC176C90E);

		private readonly List<IMonoScript> m_export = new();
		private readonly Dictionary<IUnityObjectBase, IMonoScript> m_scripts = new();
	}
}
