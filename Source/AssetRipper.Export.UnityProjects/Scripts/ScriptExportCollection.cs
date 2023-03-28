using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Utils;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated.Classes.ClassID_1035;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using System.Text;

namespace AssetRipper.Export.UnityProjects.Scripts
{
	public partial class ScriptExportCollection : ExportCollection
	{
		public ScriptExportCollection(ScriptExporter assetExporter, IMonoScript script)
		{
			AssetExporter = assetExporter ?? throw new ArgumentNullException(nameof(assetExporter));
			File = script.Collection;

			// find copies in whole project and skip them
			Dictionary<MonoScriptInfo, IMonoScript> uniqueDictionary = new();
			foreach (IMonoScript assetScript in script.Collection.Bundle.FetchAssetsInHierarchy().OfType<IMonoScript>())
			{
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

		public override bool Export(IExportContainer container, string projectDirectory)
		{
			if (m_export.Count == 0)
			{
				return false;
			}

			AssetExporter.Export(container, m_export, Path.Combine(projectDirectory, AssetsKeyword), OnScriptExported);
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
					int fileID = ComputeScriptFileID(script.Namespace_C115.String, script.ClassName_C115.String);
					return new MetaPtr(fileID, UnityEngineGUID, AssetExporter.ToExportType(asset));
				}
				else
				{
					ScriptIdentifier scriptInfo = script.GetScriptID(AssetExporter.AssemblyManager);
					if (!scriptInfo.IsDefault)
					{
						int fileID = ComputeScriptFileID(scriptInfo.Namespace, scriptInfo.Name);
						return new MetaPtr(fileID, UnityEngineGUID, AssetExporter.ToExportType(asset));
					}
				}
			}

			long exportID = GetExportID(asset);
			UnityGUID uniqueGUID = ComputeScriptGuid(script);
			return new MetaPtr(exportID, uniqueGUID, AssetExporter.ToExportType(asset));
		}

		/// <summary>
		/// Compute a unique hash of a script and use that as the Guid for the script.
		/// </summary>
		/// <remarks>
		/// This is for consistency. Script guid's are random when created in Unity.
		/// </remarks>
		private static UnityGUID ComputeScriptGuid(IMonoScript script)
		{
			//The assembly file name without any extension.
			ReadOnlySpan<byte> assemblyName = Encoding.UTF8.GetBytes(script.GetAssemblyNameFixed());
			return UnityGUID.Md5Hash(assemblyName, script.Namespace_C115.Data, script.ClassName_C115.Data);
		}

		/// <summary>
		/// Compute the FileID of a script inside an assembly.
		/// </summary>
		/// <remarks>
		/// This is a Unity algorithm.
		/// </remarks>
		private static int ComputeScriptFileID(string @namespace, string name)
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
			IMonoImporter importer = MonoImporterFactory.CreateAsset(container.ExportVersion, container.File);
			importer.ExecutionOrder_C1035 = (short)script.ExecutionOrder_C115;
			Meta meta = new Meta(ComputeScriptGuid(script), importer);
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
