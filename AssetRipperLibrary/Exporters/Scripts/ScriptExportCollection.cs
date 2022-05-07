using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Meta.Importers;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Importers;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure.Assembly;
using AssetRipper.Core.Utils;
using AssetRipper.Core.VersionHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AssetRipper.Library.Exporters.Scripts
{
	public class ScriptExportCollection : ExportCollection
	{
		public ScriptExportCollection(IAssetExporter assetExporter, IMonoScript script)
		{
			AssetExporter = assetExporter ?? throw new ArgumentNullException(nameof(assetExporter));
			File = script.SerializedFile;

			// find copies in whole project and skip them
			Dictionary<MonoScriptInfo, IMonoScript> uniqueDictionary = new();
			foreach (IUnityObjectBase asset in script.SerializedFile.Collection.FetchAssets())
			{
				if (asset is not IMonoScript assetScript)
				{
					continue;
				}

				MonoScriptInfo info = MonoScriptInfo.From(assetScript);
				if(uniqueDictionary.TryGetValue(info, out IMonoScript uniqueScript))
				{
					m_scripts.Add(assetScript, uniqueScript);
				}
				else
				{
					m_scripts.Add(assetScript, assetScript);
					uniqueDictionary.Add(info, assetScript);
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

			string scriptFolder = m_export[0].ExportPath;
			string scriptPath = Path.Combine(dirPath, scriptFolder);

			AssetExporter.Export(container, m_export, scriptPath, OnScriptExported);
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
			if (!MonoScriptExtensions.HasAssemblyName(script.SerializedFile.Version, script.SerializedFile.Flags) || s_unityEngine.IsMatch(script.GetAssemblyNameFixed()))
			{
				if (MonoScriptExtensions.HasNamespace(script.SerializedFile.Version))
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
			using HashAlgorithm hash = new MD4();
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
			IMonoImporter importer = ImporterVersionHandler.GetImporterFactory(container.ExportVersion).CreateMonoImporter(container.ExportLayout);
			importer.ExecutionOrder = (short)script.ExecutionOrder;
			Meta meta = new Meta(script.GUID, importer);
			ExportMeta(container, meta, path);
		}

		public override IAssetExporter AssetExporter { get; }
		public override ISerializedFile File { get; }
		public override IEnumerable<IUnityObjectBase> Assets => m_scripts.Keys;
		public override string Name => nameof(ScriptExportCollection);

		private static readonly UnityGUID UnityEngineGUID = new UnityGUID(0x1F55507F, 0xA1948D44, 0x4080F528, 0xC176C90E);
		private static readonly Regex s_unityEngine = new Regex(@"^UnityEngine(\.[0-9a-zA-Z]+)*(\.dll)?$", RegexOptions.Compiled);

		private readonly List<IMonoScript> m_export = new List<IMonoScript>();
		private readonly Dictionary<IUnityObjectBase, IMonoScript> m_scripts = new Dictionary<IUnityObjectBase, IMonoScript>();

		private struct MonoScriptInfo : IEquatable<MonoScriptInfo>
		{
			public readonly string @class;
			public readonly string @namespace;
			public readonly string assembly;

			public MonoScriptInfo(string @class, string @namespace, string assembly)
			{
				this.@class = @class;
				this.@namespace = @namespace;
				this.assembly = assembly;
			}

			public static MonoScriptInfo From(IMonoScript monoScript)
			{
				return new MonoScriptInfo(monoScript.ClassName, monoScript.Namespace, monoScript.GetAssemblyNameFixed());
			}

			public override bool Equals(object obj)
			{
				return obj is MonoScriptInfo info && Equals(info);
			}

			public bool Equals(MonoScriptInfo other)
			{
				return @class == other.@class &&
					   @namespace == other.@namespace &&
					   assembly == other.assembly;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(@class, @namespace, assembly);
			}

			public static bool operator ==(MonoScriptInfo left, MonoScriptInfo right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(MonoScriptInfo left, MonoScriptInfo right)
			{
				return !(left == right);
			}
		}
	}
}
